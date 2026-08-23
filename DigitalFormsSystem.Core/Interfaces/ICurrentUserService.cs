using System;

namespace DigitalFormsSystem.Core.Interfaces
{
    public interface ICurrentUserService
    {
        int? EmployeeId { get; }
        string? EmployeeName { get; }
        string? EmployeeNo { get; }
        string? EmployeeDepartment { get; }
        bool IsAuthenticated { get; }

        void SignIn(int employeeId, string name, string employeeNo, string department);
        void SignOut();
    }
}