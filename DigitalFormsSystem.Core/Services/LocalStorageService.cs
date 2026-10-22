using DigitalFormsSystem.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalFormsSystem.Core.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LocalStorageService> _logger;
        private readonly string _uploadsFolder = "uploads";

        public LocalStorageService(IWebHostEnvironment env, ILogger<LocalStorageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<StoredFile> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
        {
            // Path: wwwroot/uploads/{folder}/ (e.g., wwwroot/uploads/PartI)
            var uploadsPath = Path.Combine(_env.WebRootPath, _uploadsFolder, folder);
            
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename to prevent overwriting
            var uniqueFileName = $"{System.Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream, ct);
            }

            _logger.LogInformation("File saved locally: {FilePath}", filePath);

            // Return the StoredFile record exactly as defined in your IStorageService interface
            return new StoredFile(
                FileId: uniqueFileName, 
                PublicUrl: $"/{_uploadsFolder}/{folder}/{uniqueFileName}"
            );
        }

        public async Task DeleteAsync(string storageFileId, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(storageFileId)) return;

            // Search for the file in all folder directories inside wwwroot/uploads
            var uploadsRoot = Path.Combine(_env.WebRootPath, _uploadsFolder);
            
            if (Directory.Exists(uploadsRoot))
            {
                foreach (var folderDir in Directory.GetDirectories(uploadsRoot))
                {
                    var filePath = Path.Combine(folderDir, storageFileId);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        _logger.LogInformation("File deleted locally: {FilePath}", filePath);
                        return;
                    }
                }
            }
            
            _logger.LogWarning("File not found for deletion: {FileId}", storageFileId);
            await Task.CompletedTask;
        }
    }
}