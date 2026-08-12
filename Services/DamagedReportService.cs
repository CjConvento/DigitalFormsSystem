using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DigitalFormsSystem.Web.Services
{
    public class DamagedReportService : IDamagedReportService
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly IConfiguration _config;

        public DamagedReportService(DigitalFormsSystemContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // ============ READ ============
        public async Task<List<DamagedReport>> GetUserReportsAsync(int employeeId)
        {
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
            uploadsPath ??= "uploads/damagedreports";
            report.ControlNo = GenerateControlNo();
            report.CreatedAt = DateTime.Now;
            report.UpdatedAt = DateTime.Now;
            report.RequestStatus = "Draft";

            _context.DamagedReports.Add(report);
            await _context.SaveChangesAsync();

            var uploadsFolder = GetUploadsFolder(webRootPath, uploadsPath);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            if (partIimages != null)
            {
                foreach (var img in partIimages)
                {
                    if (img.Length > 0)
                    {
                        var imageRecord = await SaveImageAsync(img, report.Id, "PartI", uploadsFolder, uploadsPath);
                        _context.DamagedReportImages.Add(imageRecord);
                    }
                }
            }

            if (partIIimages != null)
            {
                foreach (var img in partIIimages)
                {
                    if (img.Length > 0)
                    {
                        var imageRecord = await SaveImageAsync(img, report.Id, "PartII", uploadsFolder, uploadsPath);
                        _context.DamagedReportImages.Add(imageRecord);
                    }
                }
            }

            await _context.SaveChangesAsync();
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
                    var fullPath = Path.Combine(GetUploadsFolder(webRootPath, uploadsPath), Path.GetFileName(img.FilePath));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
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

            var uploadsFolder = GetUploadsFolder(webRootPath, uploadsPath);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            if (partIimages != null && partIimages.Any())
            {
                foreach (var img in partIimages)
                {
                    if (img.Length > 0)
                    {
                        var imageRecord = await SaveImageAsync(img, existing.Id, "PartI", uploadsFolder, uploadsPath);
                        _context.DamagedReportImages.Add(imageRecord);
                    }
                }
                await _context.SaveChangesAsync();
            }

            if (partIIimages != null && partIIimages.Any())
            {
                foreach (var img in partIIimages)
                {
                    if (img.Length > 0)
                    {
                        var imageRecord = await SaveImageAsync(img, existing.Id, "PartII", uploadsFolder, uploadsPath);
                        _context.DamagedReportImages.Add(imageRecord);
                    }
                }
                await _context.SaveChangesAsync();
            }

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

            var uploadsFolder = GetUploadsFolder(webRootPath, uploadsPath);
            foreach (var img in report.Images)
            {
                var fullPath = Path.Combine(uploadsFolder, Path.GetFileName(img.FilePath));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
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
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report != null && report.Images == null)
                report.Images = new List<DamagedReportImage>();

            return report;
        }

        // ============ HELPERS ============
        private async Task<DamagedReportImage> SaveImageAsync(
            IFormFile file,
            int reportId,
            string section,
            string uploadsFolder,
            string uploadsPath)
        {
            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new DamagedReportImage
            {
                DamagedReportId = reportId,
                Section = section,
                FileName = file.FileName,
                FilePath = $"/{uploadsPath}/{uniqueName}",
                ContentType = file.ContentType,
                UploadedAt = DateTime.Now
            };
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