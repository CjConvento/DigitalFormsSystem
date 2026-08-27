using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting; 
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client.Extensions.Msal;

namespace DigitalFormsSystem.Web.Services
{
    public class DamagedReportService : IDamagedReportService
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DamagedReportService> _logger;
        private readonly IConfiguration _config;
        private readonly int _managerId;  

        public DamagedReportService(
            DigitalFormsSystemContext context, 
            IWebHostEnvironment env,
            IConfiguration config,
            ILogger<DamagedReportService> logger
            )
        {
            _context = context;
            _env = env;
            _config = config;
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

            var storageProvider = _config["StorageSettings:Provider"] ?? "LocalIIS";
            var connString = _config["StorageSettings:ConnectionString"];
            var containerName = _config["StorageSettings:ContainerName"] ?? "damaged-reports";

            _logger.LogInformation("Saving {Count} images for report {ReportId} using provider: {Provider}", 
            images.Count, reportId, storageProvider);

            var uploadsFolder = GetUploadsFolder(webRootPath, uploadsPath);

            if (storageProvider == "AzureBlob" && !string.IsNullOrEmpty(connString))
            {
                // ☁️ AZURE BLOB LOGIC (BAGO)
                var blobServiceClient = new BlobServiceClient(connString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                foreach (var file in images)
                {
                    if (file.Length == 0) continue;
                    if (!IsValidImage(file, out _, maxFileSizeMB)) continue;

                    var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var blobClient = containerClient.GetBlobClient($"{section.ToLower()}/{uniqueName}");

                    using (var stream = file.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, new BlobUploadOptions
                        {
                            HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
                        });
                    }

                    var imageRecord = new DamagedReportImage
                    {
                        DamagedReportId = reportId,
                        Section = section,
                        FileName = file.FileName,
                        FilePath = blobClient.Uri.ToString(), // ✅ IBA: Buong URL sa Azure
                        ContentType = file.ContentType,
                        UploadedAt = DateTime.Now
                    };
                    _context.DamagedReportImages.Add(imageRecord);
                }
            }
            else
            {
                // 💻 LOCAL IIS LOGIC (KAPAREHO NG LUMANG SaveImageAsync)
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in images)
                {
                    if (file.Length == 0) continue;
                    if (!IsValidImage(file, out _, maxFileSizeMB)) continue;

                    var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var imageRecord = new DamagedReportImage
                    {
                        DamagedReportId = reportId,
                        Section = section,
                        FileName = file.FileName,
                        FilePath = $"/{uploadsPath}/{uniqueName}", // ✅ KAPAREHO NG LUMANG FORMAT
                        ContentType = file.ContentType,
                        UploadedAt = DateTime.Now
                    };
                    _context.DamagedReportImages.Add(imageRecord);
                }
            }

            await _context.SaveChangesAsync();
        }

        // ✅ BAGONG HELPER
        private async Task DeleteImageAsync(DamagedReportImage image, string webRootPath, string uploadsPath)
        {
            var storageProvider = _config["StorageSettings:Provider"] ?? "LocalIIS";
            var connString = _config["StorageSettings:ConnectionString"];
            var containerName = _config["StorageSettings:ContainerName"] ?? "damaged-reports";

            if (storageProvider == "AzureBlob" && !string.IsNullOrEmpty(connString))
            {
                // ☁️ BURAHIN MULA SA AZURE
                try
                {
                    var uri = new Uri(image.FilePath);
                    var blobClient = new BlobClient(connString, containerName, uri.Segments.Last());
                    
                    var response = await blobClient.DeleteIfExistsAsync();

                    if (response.Value)
                    {
                        _logger.LogInformation("Successfully deleted blob: {BlobName} for report {ReportId}", 
                            uri.Segments.Last(), image.DamagedReportId);
                    }
                    else
                    {
                        _logger.LogWarning("Blob not found: {BlobName} for report {ReportId}", 
                            uri.Segments.Last(), image.DamagedReportId);
                    }
                }
                catch (Exception ex)
                { 
                // Log the error with context
                _logger.LogError(ex, "Failed to delete blob from Azure. BlobName: {BlobName}, ReportId: {ReportId}, FilePath: {FilePath}", 
                    Path.GetFileName(image.FilePath), 
                    image.DamagedReportId, 
                    image.FilePath);
                
                // Optional: Re-throw if you want the operation to fail
                // throw;    
                }
            }
            else
            {
                try
                {
                    // 💻 BURAHIN MULA SA LOCAL FILESYSTEM
                    var uploadsFolder = GetUploadsFolder(webRootPath, uploadsPath);
                    var fileName = Path.GetFileName(image.FilePath);
                    var fullPath = Path.Combine(uploadsFolder, fileName);


                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        _logger.LogInformation("Successfully deleted local file: {FilePath}", fullPath);
                    }
                    else
                    {
                        _logger.LogWarning("Local file not found: {FilePath}", fullPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete local file. FilePath: {FilePath}, ReportId: {ReportId}", 
                        image.FilePath, image.DamagedReportId);
                }
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