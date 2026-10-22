using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace DigitalFormsSystem.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly IAuditService _auditService;

        public AccountService(DigitalFormsSystemContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<(bool Success, int? EmployeeId, string? Name, string? EmployeeNo, string? Department, string? ErrorMessage)> LoginAsync(string employeeNo, string password)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeNo == employeeNo);

            if (employee == null)
            {
                await _auditService.LogAsync("LoginFailed", "Account", null, $"Employee not found: {employeeNo}");
                return (false, null, null, null, null, "Employee not found.");
            }

            if (string.IsNullOrEmpty(employee.PasswordHash))
            {
                await _auditService.LogAsync("LoginFailed", "Account", employee.Id, $"Missing password hash for {employeeNo}");
                return (false, employee.Id, null, null, null, "Account configuration error.");
            }

            bool isValid = BCrypt.Net.BCrypt.Verify(password, employee.PasswordHash);

            if (isValid)
            {
                await _auditService.LogAsync("Login", "Account", employee.Id, $"Login successful for {employeeNo}");
                return (true, employee.Id, employee.Name, employee.EmployeeNo, employee.Department, null);
            }

            await _auditService.LogAsync("LoginFailed", "Account", employee.Id, $"Failed login attempt for {employeeNo}");
            return (false, employee.Id, null, null, null, "Invalid password.");
        }

        public async Task LogoutAsync(int? employeeId, string? employeeName)
        {
            await _auditService.LogAsync("Logout", "Account", employeeId, $"Logout for {employeeName}");
        }
    }
}