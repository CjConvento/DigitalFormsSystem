using DigitalFormsSystem.Core.Dtos;
using DigitalFormsSystem.Core.Models;

namespace DigitalFormsSystem.Core.Interfaces
{
    public interface IFixedAssetRequestService
    {
        // ========================
        // READ 
        // =======================

        Task<List<FixedAssetRequest>> GetUserRequestsAsync(int employeeId);
        Task<FixedAssetRequest?> GetRequestWithDetailsAsync(int id);
        Task<bool> RequestExistsAsync(int id);

        // ========================
        // WRITE
        // ========================

        // Create
        Task<FixedAssetRequest> CreateRequestAsync(CreateFixedAssetRequestDto request, int currentEmployeeId);

        // Update
        Task<bool> UpdateRequestAsync(UpdateFixedAssetRequestDto request, int currentEmployeeId);

        // Delete
        Task<bool> DeleteRequestAsync(int id);

        // Print
        Task<FixedAssetRequest?> GetRequestForPrintAsync(int id);
        Task LogPrintActivityAsync(int requestId, int employeeId);

        // Helpers
        string GenerateControlNo(int employeeId, int retryCount);
    }
}