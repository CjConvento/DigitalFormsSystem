using DigitalFormsSystem.Core.Models;
using DigitalFormsSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using DigitalFormsSystem.Services;

namespace DigitalFormsSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;

        public AccountController(
            DigitalFormsSystemContext context, 
            ICurrentUserService currentUserService,
            IAuditService auditService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _auditService = auditService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeNo == model.EmployeeNo);

                if (employee != null && !string .IsNullOrEmpty(employee.PasswordHash))
                {

                    bool isValid = BCrypt.Net.BCrypt.Verify(model.Password, employee.PasswordHash);

                    if (isValid)
                    {
                        // ✅ SIGN IN MUNA (para may UserName na!)
                        _currentUserService.SignIn(
                            employee.Id,
                            employee.Name,
                            employee.EmployeeNo,
                            employee.Department ?? ""
                        );

                        // ✅ THEN LOG THE AUDIT (may UserName na!)
                        await _auditService.LogAsync(
                            "Login",
                            "Account",
                            employee.Id,
                            $"Login successful for {employee.EmployeeNo}"
                        );

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // ❌ Failed login (wala pang user)
                    await _auditService.LogAsync(
                        "LoginFailed",
                        "Account",
                        null,
                        $"Failed login attempt for {model.EmployeeNo}"
                    );

                    ModelState.AddModelError("", "Invalid password.");
                }
            }
            else
                {
                    // ❌ Employee not found (wala pang user)
                    await _auditService.LogAsync(
                        "LoginFailed",
                        "Account",
                        null,
                        $"Employee not found: {model.EmployeeNo}"
                    );

                    ModelState.AddModelError("", "Employee not found.");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            var empId = _currentUserService.EmployeeId;
            var empName = _currentUserService.EmployeeName;

            // ✅ LOG LOGOUT
            await _auditService.LogAsync(
                "Logout",
                "Account",
                empId,
                $"Logout for {empName}");

            _currentUserService.SignOut();
            return RedirectToAction("Login");
        }
    }
}