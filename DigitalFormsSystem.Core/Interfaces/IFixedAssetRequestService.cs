using DigitalFormsSystem.Models;

namespace DigitalFormsSystem.Core.Interfaces
{
    public interface IFixedAssetRequestService
    {
        // Read
        Task<List<FixedAssetRequest>> GetUserRequestsAsync(int employeeId);
        Task<FixedAssetRequest?> GetRequestWithDetailsAsync(int id);
        Task<bool> RequestExistsAsync(int id);

        // Create
        Task<FixedAssetRequest> CreateRequestAsync(FixedAssetRequest request, List<ExistingUnitDetail> existingUnits);

        // Update
        Task<bool> UpdateRequestAsync(FixedAssetRequest updatedRequest, List<ExistingUnitDetail> parsedUnits);

        // Delete
        Task<bool> DeleteRequestAsync(int id);

        // Print
        Task<FixedAssetRequest?> GetRequestForPrintAsync(int id);
        Task LogPrintActivityAsync(int requestId, int employeeId);

        // Helpers
        string GenerateControlNo(int employeeId);
    }
}