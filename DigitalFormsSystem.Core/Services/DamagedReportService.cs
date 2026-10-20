using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using DigitalFormsSystem.Core.Dtos;
using DigitalFormsSystem.Core.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;      
using Microsoft.Extensions.Logging; 
using Microsoft.Extensions.Hosting;

namespace DigitalFormsSystem.Core.Services
{
    public class DamagedReportService : IDamagedReportService
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly ILogger<DamagedReportService> _logger;
        private readonly IConfiguration _config;
        private readonly IStorageService _storageService;
        private readonly int _managerId;
        private readonly int _maxFileSizeMB;

        public DamagedReportService(
            DigitalFormsSystemContext context,
            IConfiguration config,
            IStorageService storageService,
            ILogger<DamagedReportService> logger
            )
        {
            _context = context;
            _config = config;
            _storageService = storageService;
            _logger = logger;
            _managerId = config.GetValue<int>("AppSettings:ManagerEmployeeId");
            _maxFileSizeMB = config.GetValue<int>("AppSettings:MaxFileSizeMB", 10); // default to 10 MB if not set
        }

        // ============ READ ============
        public async Task<List<DamagedReport>> GetUserReportsAsync(int employeeId)
        {
            var query = _context.DamagedReports.AsQueryable();

            // CHECK IF (ADMIN)
            if (employeeId == _managerId)
            {
                query = query.Where(r => r.ReportedByEmployeeId == employeeId);
            }
            return await query
                .Include(r => r.ReportedByEmployee)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<DamagedReport?> GetReportWithDetailsAsync(int id)
        {
            return await _context.DamagedReports
                .Include(r => r.ReportedByEmployee)
                .Include(r => r.ReceivedByEmployee)
                .Include(r => r.Images)
                .Include(r => r.FollowUps)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> ReportExistsAsync(int id)
        {
            return await _context.DamagedReports.AnyAsync(e => e.Id == id);
        }

        // ============ CREATE ============
        public async Task<DamagedReport> CreateReportAsync(CreateDamagedReportRequest request, int currentEmployeeId)
        {
            // 1. Validate Images upfront
            if (request.PartIImages != null) ValidateImages(request.PartIImages);
            if (request.PartIIImages != null) ValidateImages(request.PartIIImages);

            // Wrap transaction in Execution Strategy to support Npgsql retries
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // 2. Atomic Transaction: Report + Images succeed or fail together
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // DELEGATED TO MAPPER: for entity creation
                    var report = request.ToEntity(currentEmployeeId, GenerateControlNo());

                    _context.DamagedReports.Add(report);
                    await _context.SaveChangesAsync();

                    await SaveImagesAsync(request.PartIImages, report.Id, "PartI");
                    await SaveImagesAsync(request.PartIIImages, report.Id, "PartII");

                    await transaction.CommitAsync();
                    return report;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw; // let the global exception middleware handle it
                }
            });
        }

        // ============ UPDATE ============
        public async Task<bool> UpdateReportAsync(UpdateDamagedReportRequest request, int currentEmployeeId)
        {
            // Wrap transaction in Execution Strategy to support Npgsql retries
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _context.DamagedReports
                        .Include(r => r.Images)
                        .FirstOrDefaultAsync(r => r.Id == request.Id);

                    if (existing == null) throw new KeyNotFoundException("Report not found.");
                    if (existing.RequestStatus != "Draft") throw new InvalidOperationException("Only Draft reports can be updated.");

                    // Handle deletions
                    if (request.DeleteImageIds != null && request.DeleteImageIds.Any())
                    {
                        var imagesToDelete = existing.Images.Where(i => request.DeleteImageIds.Contains(i.Id)).ToList();

                        foreach (var img in imagesToDelete)
                        {
                            await _storageService.DeleteAsync(img.StorageFileId);
                            _context.DamagedReportImages.Remove(img);
                        }
                    }

                    // DELEGATED TO MAPPER: for property updates
                    existing.UpdateFrom(request);
                    await _context.SaveChangesAsync();

                    // DELEGATED TO MAPPER: for FollowUp handling
                    UpdateFollowUps(existing.Id, request.FollowUps);

                    if (request.PartIImages != null) await SaveImagesAsync(request.PartIImages, existing.Id, "PartI");
                    if (request.PartIIImages != null) await SaveImagesAsync(request.PartIIImages, existing.Id, "PartII");

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
        public async Task<bool> DeleteReportAsync(int id)
        {
            var report = await _context.DamagedReports
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return false;

            // Burahin ang lahat ng image (Azure o Local)
            foreach (var img in report.Images)
            {
                await _storageService.DeleteAsync(img.StorageFileId);
            }

            _context.DamagedReports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }

        // ============ PRINT ============
        public async Task<DamagedReport?> GetReportForPrintAsync(int id)
        {
            var report = await _context.DamagedReports
                .Include(r => r.ReportedByEmployee)
                .Include(r => r.ReceivedByEmployee)
                .Include(r => r.Images)
                .Include(r => r.FollowUps)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report != null && report.Images == null)
                report.Images = new List<DamagedReportImage>();

            return report;
        }

        // ============ HELPERS ============
        private async Task SaveImagesAsync(
            List<IFormFile>? images,
            int reportId,
            string section)
        {
            if (images == null || images.Count == 0) return;

            _logger.LogDebug("Saving {Count} images for report {ReportId} in section {Section}",
                images.Count, reportId, section);

            foreach (var file in images)
            {
                if (file.Length == 0) continue;

                try
                {
                    // 1. Upload
                    var stored = await _storageService.UploadAsync(file, section);

                    // 2. Map & Add (Delegated to Mapper)
                    var imageEntity = file.ToImageEntity(stored, reportId, section);
                    _context.DamagedReportImages.Add(imageEntity);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Image upload FAILED for report {ReportId}, section {Section}", reportId, section);
                    throw;
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<Employee>> GetEmployeesForDropdownAsync()
        {
            return await _context.Employees
                .Where(e => e.IsActive == true) // ✅ FIX: Added '== true' to handle bool?
                .OrderBy(e => e.Name)
                .Select(e => new Employee { Id = e.Id, Name = e.Name })
                .ToListAsync();
        }

        private async Task DeleteImageAsync(DamagedReportImage image, string webRootPath, string uploadsPath)
        {
            if (string.IsNullOrEmpty(image.StorageFileId))
            {
                _logger.LogDebug(
                    "Skipping storage delete for report {ReportId} — no StorageFileId (legacy image).",
                    image.DamagedReportId);
                return;
            }

            await _storageService.DeleteAsync(image.StorageFileId);
        }

        private void ValidateImages(List<IFormFile> images)
        {
            foreach (var file in images)
            {
                // Uses the class-level _maxFileSizeMB field directly
                if (file.Length > _maxFileSizeMB * 1024 * 1024)
                    throw new ArgumentException($"File {file.FileName} exceeds {_maxFileSizeMB} MB limit.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    throw new ArgumentException($"File {file.FileName} has an invalid extension.");

                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
                if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
                    throw new ArgumentException($"File {file.FileName} has an invalid content type.");
            }
        }

        public string GenerateControlNo(int retryCount = 0)
        {
            if (retryCount > 5)
                throw new Exception("Unable to generate a unique control number after 5 attempts.");

            var year = DateTime.Now.ToString("yy");
            var month = DateTime.Now.ToString("MM");
            var prefix = $"GAD-DR-{year}{month}-";

            var lastRequest = _context.DamagedReports
                .Where(r => r.ControlNo != null && r.ControlNo.StartsWith(prefix))
                .OrderByDescending(r => r.ControlNo)
                .Select(r => r.ControlNo)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastRequest != null && lastRequest.Length > prefix.Length)
            {
                if (int.TryParse(lastRequest.Substring(prefix.Length), out int lastNum))
                    nextNumber = lastNum + 1;
            }

            string newControlNo = $"{prefix}{nextNumber:D3}";

            bool alreadyExists = _context.DamagedReports.Any(r => r.ControlNo == newControlNo);
            if (alreadyExists)
            {
                return GenerateControlNo(retryCount + 1);
            }

            return newControlNo;
        }

        // Helper to map FollowUp DTOs to Entities
        private void UpdateFollowUps(int reportId, List<DamagedReportFollowUpDto>? followUpDtos)
        {
            if (followUpDtos == null) return;

            // Remove old follow-ups
            var existingFollowUps = _context.DamagedReportFollowUps
                .Where(f => f.DamagedReportId == reportId)
                .ToList();
            _context.DamagedReportFollowUps.RemoveRange(existingFollowUps);

            // Add new ones
            foreach (var dto in followUpDtos)
            {
                if (dto.FollowUpDate != default && !string.IsNullOrEmpty(dto.Status))
                {
                    _context.DamagedReportFollowUps.Add(dto.ToEntity(reportId));
                }
            }
        }
    }
}