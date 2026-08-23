using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalFormsSystem.Web.Controllers
{
    public class AuditController : Controller
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly int _managerId;

        public AuditController(
            DigitalFormsSystemContext context,
            ICurrentUserService currentUserService,
            IConfiguration configuration)
        {
            _context = context;
            _currentUserService = currentUserService;
            _managerId = configuration.GetValue<int>("AppSettings:ManagerEmployeeId");
        }

        // ✅ Only Gilbert can view audit logs
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

            var logs = await _context.AuditLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Take(100)
                .ToListAsync();

            return View(logs);
        }
    }
}