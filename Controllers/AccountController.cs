using DigitalFormsSystem.Core.Models;
using DigitalFormsSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;


namespace DigitalFormsSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditService _auditService;
        private readonly IAccountService _accountService;


        public AccountController(
            DigitalFormsSystemContext context, 
            ICurrentUserService currentUserService,
            IAccountService accountService,
            IAuditService auditService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _auditService = auditService;
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Return view ONLY if the model is INVALID
            if (!ModelState.IsValid)
            {
            return View(model);
            }

            // DELEGATED TO SERVICE: Handles DB lookup, BCrypt, and Audit Logging
            var result = await _accountService.LoginAsync(model.EmployeeNo, model.Password);

            if (result.Success)
            {
                // CONTROLLER ONLY JOB: Manage the HTTP Session
                _currentUserService.SignIn(result.EmployeeId!.Value, result.Name!, result.EmployeeNo!, result.Department ?? "");
                return RedirectToAction("Index", "Home");
            }

            // If failed, the service already logged the audit, we just show the error
            ModelState.AddModelError("", result.ErrorMessage);
            return View(model);

        }

        public async Task<IActionResult> Logout()
        {
            // LOG LOGOUT
            await _accountService.LogoutAsync(_currentUserService.EmployeeId, _currentUserService.EmployeeName);
            
            _currentUserService.SignOut();
            return RedirectToAction("Login");
        }
    }
}