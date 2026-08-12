using DigitalFormsSystem.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DigitalFormsSystem.Services   // <-- ito ang tamang namespace
{
    public class SessionCurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionCurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        public int? EmployeeId => Session?.GetInt32("EmployeeId");
        public string? EmployeeName => Session?.GetString("EmployeeName");
        public string? EmployeeNo => Session?.GetString("EmployeeNo");
        public string? EmployeeDepartment => Session?.GetString("EmployeeDepartment");
        public bool IsAuthenticated => EmployeeId != null;

        public void SignIn(int employeeId, string name, string employeeNo, string department)
        {
            Session?.SetInt32("EmployeeId", employeeId);
            Session?.SetString("EmployeeName", name);
            Session?.SetString("EmployeeNo", employeeNo);
            Session?.SetString("EmployeeDepartment", department ?? "");
        }

        public void SignOut()
        {
            Session?.Clear();
        }
    }
}