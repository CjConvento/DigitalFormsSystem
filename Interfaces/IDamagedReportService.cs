using DigitalFormsSystem.Core.Models;
using Microsoft.AspNetCore.Http;

namespace DigitalFormsSystem.Core.Interfaces
{
    public interface IDamagedReportService
    {
        // Read
        Task<List<DamagedReport>> GetUserReportsAsync(int employeeId);
        Task<DamagedReport?> GetReportWithDetailsAsync(int id);
        Task<bool> ReportExistsAsync(int id);

        // Create
        Task<DamagedReport> CreateReportAsync(
            DamagedReport report,
            List<IFormFile>? partIimages,
            List<IFormFile>? partIIimages,
            string webRootPath,
            string? uploadsPath,
            int maxFileSizeMB);

        // Update
        Task<bool> UpdateReportAsync(
            DamagedReport updatedReport,
            List<IFormFile>? partIimages,
            List<IFormFile>? partIIimages,
            List<int>? deleteImageIds,
            string webRootPath,
            string? uploadsPath,
            int maxFileSizeMB);

        // Delete
        Task<bool> DeleteReportAsync(
        int id,
        string webRootPath,
        string? uploadsPath); 

        // Print
        Task<DamagedReport?> GetReportForPrintAsync(int id);

        // Helpers
        string GenerateControlNo(int retryCount = 0);
        bool IsValidImage(IFormFile file, out string errorMessage, int maxFileSizeMB);
        string GetUploadsFolder(string webRootPath, string uploadsPath);
    }
}