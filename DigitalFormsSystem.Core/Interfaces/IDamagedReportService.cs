using DigitalFormsSystem.Core.Models;
using DigitalFormsSystem.Core.Dtos;
using Microsoft.AspNetCore.Http;

namespace DigitalFormsSystem.Core.Interfaces
{
    public interface IDamagedReportService
    {
        // ============ READ ============
        Task<List<DamagedReport>> GetUserReportsAsync(int employeeId);
        Task<DamagedReport?> GetReportWithDetailsAsync(int id, int employeeId);
        Task<bool> ReportExistsAsync(int id);
        Task<DamagedReport?> GetReportForPrintAsync(int id, int employeeId);
        Task<List<Employee>> GetEmployeesForDropdownAsync();

        // ============ WRITE (Now using DTOs, no path leakage) ============
        Task<DamagedReport> CreateReportAsync(CreateDamagedReportRequest request, int currentEmployeeId);
        Task<bool> UpdateReportAsync(UpdateDamagedReportRequest request, int currentEmployeeId);
        Task<bool> DeleteReportAsync(int id);

        // ============ HELPERS ============
        string GenerateControlNo(int retryCount = 0);
    }
}