using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalFormsSystem.Web.Controllers
{
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly DigitalFormsSystemContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly int _managerId;

        public AuditController(
            DigitalFormsSystemContext context,
            ICurrentUserService currentUserService,
            IAuditService auditService,
            IConfiguration configuration)
        {
            _context = context;
            _auditService = auditService;
            _currentUserService = currentUserService;
            _managerId = configuration.GetValue<int>("AppSettings:ManagerEmployeeId");
        }

        // ✅ Only Admin can view audit logs
        public async Task<IActionResult> Index()
        {
            if (!_currentUserService.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (_currentUserService.EmployeeId != _managerId)
            {
                TempData["ErrorMessage"] = "You are not authorized to view audit logs.";
                return RedirectToAction("Index", "Home");
            }

            // ✅ DELEGATED TO SERVICE: Controller no longer touches DbContext
            var logs = await _auditService.GetRecentLogsAsync(100);

            return View(logs);
        }
    }
}