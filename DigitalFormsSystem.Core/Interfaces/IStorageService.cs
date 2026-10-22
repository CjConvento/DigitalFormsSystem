using Microsoft.AspNetCore.Http;

namespace DigitalFormsSystem.Core.Interfaces
{
    /// <summary>
    /// Abstraction for file storage. Implementations may target
    /// local filesystem, Appwrite, Azure Blob, S3, etc.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Upload a file. Returns the storage file ID and public URL.
        /// </summary>
        Task<StoredFile> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);

        /// <summary>
        /// Delete a file by its storage file ID.
        /// </summary>
        Task DeleteAsync(string storageFileId, CancellationToken ct = default);
    }

    /// <summary>
    /// Result of an upload operation.
    /// </summary>
    public record StoredFile(string FileId, string PublicUrl);
}