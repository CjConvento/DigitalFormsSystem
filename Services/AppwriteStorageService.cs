using Appwrite;
using Appwrite.Models;
using Appwrite.Services;
using DigitalFormsSystem.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DigitalFormsSystem.Web.Services
{
    /// <summary>
    /// Appwrite-backed storage service.
    /// Requires config under StorageSettings:Appwrite:* (endpoint, projectId, apiKey, bucketId).
    /// </summary>
    public class AppwriteStorageService : IStorageService
    {
        private readonly Storage _storage;
        private readonly ILogger<AppwriteStorageService> _logger;
        private readonly string _endpoint;
        private readonly string _projectId;
        private readonly string _bucketId;

        public AppwriteStorageService(
            IConfiguration config,
            ILogger<AppwriteStorageService> logger)
        {
            _logger = logger;

                // ============================================================
                // DIAGNOSTIC — log lahat ng env vars at config keys na related
                // ============================================================
                _logger.LogInformation("=== ENV VARS SCAN START ===");
                foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
                {
                    var key = entry.Key?.ToString() ?? "";
                    if (key.Contains("Appwrite", StringComparison.OrdinalIgnoreCase)
                        || key.Contains("StorageSettings", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("ENV VAR FOUND: {Key}", key);
                    }
                }
                _logger.LogInformation("=== ENV VARS SCAN END ===");

                _logger.LogInformation("=== CONFIG KEYS SCAN START ===");
                foreach (var kvp in config.AsEnumerable())
                {
                    if (kvp.Key.Contains("Appwrite", StringComparison.OrdinalIgnoreCase)
                        || kvp.Key.Contains("StorageSettings", StringComparison.OrdinalIgnoreCase))
                    {
                        var isSecret = kvp.Key.Contains("ApiKey", StringComparison.OrdinalIgnoreCase);
                        _logger.LogInformation("CONFIG KEY: {Key} = {Value}",
                            kvp.Key,
                            isSecret ? "(hidden)" : kvp.Value);
                    }
                }
                _logger.LogInformation("=== CONFIG KEYS SCAN END ===");

            _endpoint = config["StorageSettings:Appwrite:Endpoint"] ?? "";
            _projectId = config["StorageSettings:Appwrite:ProjectId"] ?? "";
            var apiKey = config["StorageSettings:Appwrite:ApiKey"] ?? "";
            _bucketId = config["StorageSettings:Appwrite:BucketId"] ?? "damaged-reports";

            // DIAGNOSTIC LOG — makikita mo sa Render logs kung alin yung empty
            _logger.LogInformation(
                "Appwrite init: Endpoint={Endpoint} | ProjectId={ProjectId} | BucketId={BucketId} | ApiKeyLength={ApiKeyLength}",
                string.IsNullOrEmpty(_endpoint) ? "(EMPTY)" : _endpoint,
                string.IsNullOrEmpty(_projectId) ? "(EMPTY)" : _projectId,
                string.IsNullOrEmpty(_bucketId) ? "(EMPTY)" : _bucketId,
                apiKey.Length);

            var client = new Client()
                .SetEndpoint(string.IsNullOrEmpty(_endpoint) ? "https://cloud.appwrite.io/v1" : _endpoint)
                .SetProject(string.IsNullOrEmpty(_projectId) ? "placeholder" : _projectId)
                .SetKey(string.IsNullOrEmpty(apiKey) ? "placeholder" : apiKey);

            _storage = new Storage(client);
        }

        public async Task<StoredFile> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
        {
            // Read the file into memory (max 5 MB per current UploadSettings)
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms, ct);
            ms.Position = 0;

            var fileId = ID.Unique();
            var inputFile = InputFile.FromStream(ms, file.FileName, file.ContentType);

            try
            {
                var result = await _storage.CreateFile(
                    bucketId: _bucketId,
                    fileId: fileId,
                    file: inputFile
                );

                var publicUrl = BuildPublicUrl(result.Id);

                _logger.LogDebug(
                    "Appwrite upload OK. Bucket {BucketId}, FileId {FileId}, Size {Size}",
                    _bucketId, result.Id, file.Length);

                return new StoredFile(result.Id, publicUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Appwrite upload failed for bucket {BucketId}.", _bucketId);
                _logger.LogDebug(ex, "Appwrite upload exception details");
                throw;
            }
        }

        public async Task DeleteAsync(string storageFileId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(storageFileId))
                return;

            try
            {
                await _storage.DeleteFile(bucketId: _bucketId, fileId: storageFileId);

                _logger.LogDebug(
                    "Appwrite delete OK. Bucket {BucketId}, FileId {FileId}",
                    _bucketId, storageFileId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Appwrite delete failed for bucket {BucketId}.", _bucketId);
                _logger.LogDebug(ex, "Appwrite delete exception details");
                // Swallow — deletion failures are non-critical for the caller
            }
        }

        private string BuildPublicUrl(string fileId)
        {
            // https://sgp.cloud.appwrite.io/v1/storage/buckets/{bucketId}/files/{fileId}/view?project={projectId}
            return $"{_endpoint.TrimEnd('/')}/storage/buckets/{_bucketId}/files/{fileId}/view?project={_projectId}";
        }
    }
}