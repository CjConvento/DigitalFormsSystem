using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; 

namespace DigitalFormsSystem.Core.Services
{
    public class FixedAssetRequestService : IFixedAssetRequestService
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly int _managerId;

        public FixedAssetRequestService(
            DigitalFormsSystemContext context, 
            INotificationService notificationService,
            IAuditService auditService,
            IConfiguration configuration)
        {
            _context = context;
            _notificationService = notificationService;
            _auditService = auditService;
            _managerId = configuration.GetValue<int>("AppSettings:ManagerEmployeeId");

            // FOR LOGGING
            Console.WriteLine($"🔍 ManagerId loaded: {_managerId}");
        }

        // ============ READ ============
        public async Task<List<FixedAssetRequest>> GetUserRequestsAsync(int employeeId)
        {
            Console.WriteLine($"🔍 GetUserRequestsAsync called with employeeId: {employeeId}");
            Console.WriteLine($"🔍 _managerId is: {_managerId}");

            if (employeeId == _managerId)
            {
                Console.WriteLine("✅ Manager detected!");
                var allRequests = await _context.FixedAssetRequests.ToListAsync();
                Console.WriteLine($"📊 Total requests found: {allRequests.Count}");
                return allRequests;
            }
            else
            {
                Console.WriteLine("❌ Not manager. Returning only user's requests.");
                return await _context.FixedAssetRequests
                    .Where(r => r.RequestedByEmployeeId == employeeId)
                    .Include(r => r.RequestedByEmployee)
                    .OrderByDescending(r => r.DateRequested)
                    .ToListAsync();
            }
        }

        public async Task<FixedAssetRequest?> GetRequestWithDetailsAsync(int id)
        {
            return await _context.FixedAssetRequests
                .Include(r => r.RequestedByEmployee)
                .Include(r => r.EvaluatedByEmployee)
                .Include(r => r.ExistingUnitDetails)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> RequestExistsAsync(int id)
        {
            return await _context.FixedAssetRequests.AnyAsync(e => e.Id == id);
        }

        // ============ CREATE ============
        public async Task<FixedAssetRequest> CreateRequestAsync(FixedAssetRequest request, List<ExistingUnitDetail> existingUnits)
        {
            // Set default values
            request.CreatedAt = DateTime.Now;
            request.UpdatedAt = DateTime.Now;
            request.RequestStatus = "Draft";
            request.RequestedAt = DateTime.Now;
            request.DateRequested = DateOnly.FromDateTime(DateTime.Now);

            // Generate ControlNo with retry logic (max 3 attempts)
            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    request.ControlNo = GenerateControlNo(request.RequestedByEmployeeId);
                    _context.FixedAssetRequests.Add(request);
                    await _context.SaveChangesAsync();

                    // After save
                    await _notificationService.NotifyStatusChangeAsync(
                        request.Id, 
                        "Draft", 
                        "Draft", 
                        request.RequestedByEmployeeId);

                    // Save existing units if Additional
                    if (request.RequestType == "Additional" && existingUnits.Any())
                    {
                        foreach (var unit in existingUnits)
                        {
                            unit.FixedAssetRequestId = request.Id;
                            _context.ExistingUnitDetails.Add(unit);
                        }
                        await _context.SaveChangesAsync();
                    }

                    return request;
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UQ_FixedAssetRequests_ControlNo") == true)
                {
                    if (attempt == maxRetries) throw;
                    // Otherwise, retry with a new ControlNo (recursive call will generate new number)
                }
            }
            throw new Exception("Unable to create request after multiple attempts.");
        }

        // ============ UPDATE ============
        public async Task<bool> UpdateRequestAsync(FixedAssetRequest updatedRequest, List<ExistingUnitDetail> parsedUnits)
        {
            var existing = await _context.FixedAssetRequests
                .Include(r => r.ExistingUnitDetails)
                .FirstOrDefaultAsync(r => r.Id == updatedRequest.Id);

            if (existing == null) return false;

            // Update scalar fields
            existing.Department = updatedRequest.Department;
            existing.Section = updatedRequest.Section;
            existing.TargetDateNeeded = updatedRequest.TargetDateNeeded;
            existing.Quantity = updatedRequest.Quantity;
            existing.AssetType = updatedRequest.AssetType;
            existing.DetailedDescription = updatedRequest.DetailedDescription;
            existing.ReasonPurpose = updatedRequest.ReasonPurpose;
            existing.ProposedLocation = updatedRequest.ProposedLocation;
            existing.EstimatedLifeSpan = updatedRequest.EstimatedLifeSpan;
            existing.RequestType = updatedRequest.RequestType;
            existing.DamagedReportNo = updatedRequest.DamagedReportNo;
            existing.EvaluatedByName = updatedRequest.EvaluatedByName;
            existing.UpdatedAt = DateTime.Now;

            // Replace ExistingUnitDetails
            _context.ExistingUnitDetails.RemoveRange(existing.ExistingUnitDetails);

            if (existing.RequestType == "Additional" && parsedUnits.Any())
            {
                foreach (var unit in parsedUnits)
                {
                    unit.FixedAssetRequestId = existing.Id;
                    _context.ExistingUnitDetails.Add(unit);
                }
            }

            await _context.SaveChangesAsync();

            // After update, check if status changed
            if (existing.RequestStatus != updatedRequest.RequestStatus)
            {
                await _notificationService.NotifyStatusChangeAsync(
                    existing.Id,
                    existing.RequestStatus ?? "Unknown",   
                    updatedRequest.RequestStatus ?? "Unknown",  
                    existing.RequestedByEmployeeId);
            }

            return true;
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
        public async Task<FixedAssetRequest?> GetRequestForPrintAsync(int id)
        {
            return await _context.FixedAssetRequests
                .Include(r => r.RequestedByEmployee)
                .Include(r => r.EvaluatedByEmployee)
                .Include(r => r.ExistingUnitDetails)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task LogPrintActivityAsync(int requestId, int employeeId)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_PrintFixedAssetRequest @RequestID={0}, @PrintedByEmployeeID={1}",
                    requestId, employeeId);
            }
            catch
            {
                // Ignore logging errors – print activity is non-critical
            }
        }

        // ============ HELPERS ============
        public string GenerateControlNo(int employeeId)
        {
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
                return GenerateControlNo(employeeId); // recursive retry
            }

            return newControlNo;
        }
    }
}