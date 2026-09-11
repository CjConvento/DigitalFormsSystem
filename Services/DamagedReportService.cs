using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting; 
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace DigitalFormsSystem.Web.Services
{
    public class DamagedReportService : IDamagedReportService
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DamagedReportService> _logger;
        private readonly IConfiguration _config;
        private readonly IStorageService _storageService;
        private readonly int _managerId;  

        public DamagedReportService(
            DigitalFormsSystemContext context, 
            IWebHostEnvironment env,
            IConfiguration config,
            IStorageService storageService,
            ILogger<DamagedReportService> logger
            )
        {
            _context = context;
            _env = env;
            _config = config;
            _storageService = storageService;
            _logger = logger;
            _managerId = config.GetValue<int>("AppSettings:ManagerEmployeeId");
        }

        // ============ READ ============
        public async Task<List<DamagedReport>> GetUserReportsAsync(int employeeId)
        {
            // ✅ CHECK IF GILBERT (ADMIN)
            if (employeeId == _managerId)
            {
                // ✅ RETURN ALL REPORTS
                return await _context.DamagedReports
                    .Include(r => r.ReportedByEmployee)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }

            // ✅ REGULAR USER: ONLY THEIR OWN
            return await _context.DamagedReports
                .Include(r => r.ReportedByEmployee)
                .Where(r => r.ReportedByEmployeeId == employeeId)
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
        public async Task<DamagedReport> CreateReportAsync(
            DamagedReport report,
            List<IFormFile>? partIimages,
            List<IFormFile>? partIIimages,
            string webRootPath,
            string? uploadsPath,
            int maxFileSizeMB)
        {
                // 1. Setup ng report (ISANG BESES LANG)
                uploadsPath ??= "uploads/damagedreports";
                report.ControlNo = GenerateControlNo();
                report.CreatedAt = DateTime.Now;
                report.UpdatedAt = DateTime.Now;
                report.RequestStatus = "Draft";

                _context.DamagedReports.Add(report);
                await _context.SaveChangesAsync();

                // 2. Mag-save ng images (ISANG BESES LANG — depende sa provider)
                await SaveImagesAsync(partIimages, report.Id, "PartI", webRootPath, uploadsPath, maxFileSizeMB);
                await SaveImagesAsync(partIIimages, report.Id, "PartII", webRootPath, uploadsPath, maxFileSizeMB);

                return report;
            }

        // ============ UPDATE ============
        public async Task<bool> UpdateReportAsync(
            DamagedReport updatedReport,
            List<IFormFile>? partIimages,
            List<IFormFile>? partIIimages,
            List<int>? deleteImageIds,
            string webRootPath,
            string? uploadsPath,
            int maxFileSizeMB)
        {
            uploadsPath ??= "uploads/damagedreports";

            var existing = await _context.DamagedReports
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == updatedReport.Id);

            if (existing == null) return false;
            if (existing.RequestStatus != "Draft") return false;

            if (deleteImageIds != null && deleteImageIds.Any())
            {
                var imagesToDelete = existing.Images.Where(i => deleteImageIds.Contains(i.Id)).ToList();
                
                foreach (var img in imagesToDelete)
                {
                    await DeleteImageAsync(img, webRootPath, uploadsPath);
                    _context.DamagedReportImages.Remove(img);
                }
                await _context.SaveChangesAsync();
            }

            existing.Item = updatedReport.Item;
            existing.FixedAssetCode = updatedReport.FixedAssetCode;
            existing.DatePurchased = updatedReport.DatePurchased;
            existing.BrandSize = updatedReport.BrandSize;
            existing.LocationUser = updatedReport.LocationUser;
            existing.SerialNumber = updatedReport.SerialNumber;
            existing.Color = updatedReport.Color;
            existing.IncidentDateTime = updatedReport.IncidentDateTime;
            existing.CauseOfDamage = updatedReport.CauseOfDamage;
            existing.ImmediateAction = updatedReport.ImmediateAction;
            existing.RecommendedAction = updatedReport.RecommendedAction;
            existing.ReceivedByEmployeeId = updatedReport.ReceivedByEmployeeId;
            existing.ReceivedDateTime = updatedReport.ReceivedDateTime;
            existing.UpdatedAt = DateTime.Now;

            var isGad = !string.IsNullOrEmpty(updatedReport.Findings) ||
                        !string.IsNullOrEmpty(updatedReport.Recommendation) ||
                        updatedReport.InvestigatedByEmployeeId != null ||
                        updatedReport.VerifiedByEmployeeId != null ||
                        updatedReport.NotedByEmployeeId != null;

            if (isGad)
            {
                existing.Findings = updatedReport.Findings;
                existing.Recommendation = updatedReport.Recommendation;
                existing.NegligenceFlag = updatedReport.NegligenceFlag;
                existing.NegligenceDetails = updatedReport.NegligenceDetails;
                existing.Remarks = updatedReport.Remarks;
                existing.AdministrativeDiscipline = updatedReport.AdministrativeDiscipline;
                existing.InvestigatedByEmployeeId = updatedReport.InvestigatedByEmployeeId;
                existing.VerifiedByEmployeeId = updatedReport.VerifiedByEmployeeId;
                existing.NotedByEmployeeId = updatedReport.NotedByEmployeeId;
            }

            await _context.SaveChangesAsync();

            // Magdagdag ng mga bagong imahe
            await SaveImagesAsync(partIimages, existing.Id, "PartI", webRootPath, uploadsPath, maxFileSizeMB);
            await SaveImagesAsync(partIIimages, existing.Id, "PartII", webRootPath, uploadsPath, maxFileSizeMB);

            return true;
        }

        // ============ DELETE ============
        public async Task<bool> DeleteReportAsync(int id, string webRootPath, string? uploadsPath)
        {
            uploadsPath ??= "uploads/damagedreports";

            var report = await _context.DamagedReports
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return false;

            // Burahin ang lahat ng imahe (Azure o Local)
            foreach (var img in report.Images)
            {
                await DeleteImageAsync(img, webRootPath, uploadsPath);
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
            List <IFormFile>? images,
            int reportId,
            string section,
            string webRootPath,
            string uploadsPath,
            int maxFileSizeMB)
        {
            if (images == null || images.Count == 0) return;

            _logger.LogDebug("Saving {Count} images for report {ReportId} in section {Section}",
                images.Count, reportId, section);

            foreach (var file in images)
            {
                if (file.Length == 0) continue;
                if (!IsValidImage(file, out _, maxFileSizeMB)) continue;

                try
                {
                    // Upload to storage provider (Appwrite)
                    var stored = await _storageService.UploadAsync(file, section);

                    _context.DamagedReportImages.Add(new DamagedReportImage
                    {
                        DamagedReportId = reportId,
                        Section = section,
                        FileName = file.FileName,
                        FilePath = stored.PublicUrl,
                        StorageFileId = stored.FileId,
                        ContentType = file.ContentType,
                        UploadedAt = DateTime.Now
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                    "Image upload FAILED for report {ReportId}, section {Section}",
                    reportId, section);
                }
            }

            await _context.SaveChangesAsync();
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

        public bool IsValidImage(IFormFile file, out string errorMessage, int maxFileSizeMB)
        {
            errorMessage = null!;

            if (file.Length > maxFileSizeMB * 1024 * 1024)
            {
                errorMessage = $"File {file.FileName} exceeds {maxFileSizeMB} MB limit.";
                return false;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                errorMessage = $"File {file.FileName} has an invalid extension. Allowed: {string.Join(", ", allowedExtensions)}";
                return false;
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                errorMessage = $"File {file.FileName} has an invalid content type.";
                return false;
            }

            return true;
        }

        public string GetUploadsFolder(string webRootPath, string uploadsPath)
        {
            return Path.Combine(webRootPath, uploadsPath);
        }
    }
}