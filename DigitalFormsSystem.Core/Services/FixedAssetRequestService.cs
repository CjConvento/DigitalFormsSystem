using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using DigitalFormsSystem.Core.Dtos;
using DigitalFormsSystem.Core.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DigitalFormsSystem.Core.Services
{
    public class FixedAssetRequestService : IFixedAssetRequestService
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILogger<FixedAssetRequestService> _logger;
        private readonly int _managerId;

        public FixedAssetRequestService(
            DigitalFormsSystemContext context,
            INotificationService notificationService,
            IAuditService auditService,
            ILogger<FixedAssetRequestService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _notificationService = notificationService;
            _auditService = auditService;
            _logger = logger;
            _managerId = configuration.GetValue<int>("AppSettings:ManagerEmployeeId");

            // FOR LOGGING
            _logger.LogDebug("🔍 ManagerId loaded: {ManagerId}", _managerId);
        }

        // ============ READ ============
        public async Task<List<FixedAssetRequest>> GetUserRequestsAsync(int employeeId)
        {
            var query = _context.FixedAssetRequests.AsQueryable();

            // RBAC: If the user is not the manager, filter to only their own requests
            if (employeeId != _managerId)
            {
                query = query.Where(r => r.RequestedByEmployeeId == employeeId);
            }
            return await query.Include(r => r.RequestedByEmployee).OrderByDescending(r => r.DateRequested).ToListAsync();
        }

        public async Task<FixedAssetRequest?> GetRequestWithDetailsAsync(int id, int employeeId)
        {
            var query = _context.FixedAssetRequests.AsQueryable();

            // SECURITY CHECK: If NOT admin, they can only fetch their own request
            if (employeeId != _managerId)
            {
                query = query.Where(r => r.RequestedByEmployeeId == employeeId);
            }

            return await query
                .Include(r => r.RequestedByEmployee)
                .Include(r => r.EvaluatedByEmployee)
                .Include(r => r.ExistingUnitDetails)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> RequestExistsAsync(int id) => await _context.FixedAssetRequests.AnyAsync(e => e.Id == id);

        // ============ CREATE ============
        public async Task<FixedAssetRequest> CreateRequestAsync(CreateFixedAssetRequestDto request, int currentEmployeeId)
        {
            // Wrap transaction in Execution Strategy to support Npgsql retries
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // DELEGATED TO MAPPER
                    var entity = request.ToEntity(currentEmployeeId, GenerateControlNo(currentEmployeeId));

                    _context.FixedAssetRequests.Add(entity);
                    await _context.SaveChangesAsync(); // Get entity.Id

                    // Handle nested ExistingUnitDetails inside the SAME transaction
                    if (request.ExistingUnits != null && request.ExistingUnits.Any())
                    {
                        int itemNo = 1;
                        foreach (var dto in request.ExistingUnits)
                        {
                            _context.ExistingUnitDetails.Add(dto.ToEntity(entity.Id, itemNo++));
                        }
                        await _context.SaveChangesAsync();
                    }

                    await _notificationService.NotifyStatusChangeAsync(entity.Id, "Draft", "Draft", currentEmployeeId);
                    await transaction.CommitAsync();

                    return entity;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw; // Global Exception Middleware will handle it
                }
            });
        }

        // ============ UPDATE ============
        public async Task<bool> UpdateRequestAsync(UpdateFixedAssetRequestDto request, int currentEmployeeId)
        {
            // Wrap transaction in Execution Strategy to support Npgsql retries
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _context.FixedAssetRequests
                        .Include(r => r.ExistingUnitDetails)
                        .FirstOrDefaultAsync(r => r.Id == request.Id)
                        ?? throw new KeyNotFoundException("Request not found.");

                    if (existing.RequestStatus != "Draft")
                        throw new InvalidOperationException("Only Draft requests can be updated.");

                    // 1. Update main entity (DELEGATED TO MAPPER)
                    existing.UpdateFrom(request);

                    // 2. Handle Nested ExistingUnitDetails (Clear old, add new)
                    _context.ExistingUnitDetails.RemoveRange(existing.ExistingUnitDetails);

                    if (request.ExistingUnits != null && request.ExistingUnits.Any())
                    {
                        int itemNo = 1;
                        foreach (var dto in request.ExistingUnits)
                        {
                            _context.ExistingUnitDetails.Add(dto.ToEntity(existing.Id, itemNo++));
                        }
                    }

                    await _context.SaveChangesAsync();

                    if (existing.RequestStatus != request.RequestType) // Simplified status check
                    {
                        await _notificationService.NotifyStatusChangeAsync(existing.Id, existing.RequestStatus ?? "Unknown", existing.RequestStatus ?? "Unknown", currentEmployeeId);
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        // ============ DELETE ============
        public async Task<bool> DeleteRequestAsync(int id)
        {
            var request = await _context.FixedAssetRequests
                .Include(r => r.ExistingUnitDetails)
                .Include(r => r.FixedAssetRequestApprovals)
                .Include(r => r.MemorandumReceipts)
                .Include(r => r.FixedAssetPrintLogs)
                .Include(r => r.RequestStatusHistories)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return false;
            if (request.RequestStatus != "Draft")
                throw new InvalidOperationException("Only Draft requests can be deleted.");

            // Remove child records
            _context.ExistingUnitDetails.RemoveRange(request.ExistingUnitDetails ?? new List<ExistingUnitDetail>());
            _context.FixedAssetRequestApprovals.RemoveRange(request.FixedAssetRequestApprovals ?? new List<FixedAssetRequestApproval>());
            _context.MemorandumReceipts.RemoveRange(request.MemorandumReceipts ?? new List<MemorandumReceipt>());
            _context.FixedAssetPrintLogs.RemoveRange(request.FixedAssetPrintLogs ?? new List<FixedAssetPrintLog>());
            _context.RequestStatusHistories.RemoveRange(request.RequestStatusHistories ?? new List<RequestStatusHistory>());

            _context.FixedAssetRequests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }

        // ============ PRINT ============
        public async Task<FixedAssetRequest?> GetRequestForPrintAsync(int id, int employeeId)
        {
            var query = _context.FixedAssetRequests.AsQueryable();

            // SECURITY CHECK: If NOT admin, they can only fetch their own request for printing
            if (employeeId != _managerId)
            {
                query = query.Where(r => r.RequestedByEmployeeId == employeeId);
            }

            return await query
                .Include(r => r.RequestedByEmployee)
                .Include(r => r.EvaluatedByEmployee)
                .Include(r => r.ExistingUnitDetails)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task LogPrintActivityAsync(int requestId, int employeeId)
        {
            try
            {
                var log = new FixedAssetPrintLog
                {
                    FixedAssetRequestId = requestId,
                    PrintedByEmployeeId = employeeId,
                    PrintDateTime = DateTime.Now,
                    PrintFormat = "Full Form"
                };
                _context.FixedAssetPrintLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Print activity logged for request {RequestId}", requestId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to log print activity for request {RequestId}", requestId);
                _logger.LogDebug(ex, "Print log exception details");
            }
        }

        // ============ HELPERS ============

        public string GenerateControlNo(int employeeId, int retryCount = 0)
        {
            if (retryCount > 5) throw new InvalidOperationException("Unable to generate unique control number after 5 attempts.");

            var employee = _context.Employees.Find(employeeId);
            string location = employee?.Location?.Trim() ?? "F1";
            string year = DateTime.Now.ToString("yy");
            string prefix = $"GAD-FAR-{location}-{year}-";

            var lastRequest = _context.FixedAssetRequests
                .Where(r => r.ControlNo != null && r.ControlNo.StartsWith(prefix))
                .OrderByDescending(r => r.ControlNo)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastRequest != null && lastRequest.ControlNo != null)
            {
                string lastNumberStr = lastRequest.ControlNo.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNum))
                    nextNumber = lastNum + 1;
            }

            string newControlNo = $"{prefix}{nextNumber:D3}";

            // Avoid race condition
            bool alreadyExists = _context.FixedAssetRequests.Any(r => r.ControlNo == newControlNo);
            if (alreadyExists)
            {
                return GenerateControlNo(employeeId, retryCount + 1); // recursive retry
            }

            return newControlNo;
        }
    }
}