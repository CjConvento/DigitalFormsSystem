using System.Threading.Tasks;

namespace DigitalFormsSystem.Core.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Success, int? EmployeeId, string? Name, string? EmployeeNo, string? Department, string? ErrorMessage)> LoginAsync(string employeeNo, string password);
        Task LogoutAsync(int? employeeId, string? employeeName);
    }
}