using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
using DigitalFormsSystem.Core.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DigitalFormsSystem.Controllers
{
    public class DamagedReportController : Controller
    {
        private readonly IDamagedReportService _service;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DamagedReportController> _logger;
        private readonly IAuditService _auditService;

        public DamagedReportController(
            IDamagedReportService service,
            ICurrentUserService currentUserService,
            IAuditService auditService,
            IWebHostEnvironment env,
            ILogger<DamagedReportController> logger)
        {
            _service = service;
            _currentUserService = currentUserService;
            _auditService = auditService;
            _logger = logger;
        }

        // ============================================================
        // 📋 READ OPERATIONS
        // ============================================================

        // ============ INDEX ============
        public async Task<IActionResult> Index()
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var reports = await _service.GetUserReportsAsync(_currentUserService.EmployeeId!.Value);
            return View(reports);
        }

        // ============ DETAILS ============
        public async Task<IActionResult> Details(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            // Pass EmployeeId to Service. Service enforces RBAC.
            var report = await _service.GetReportWithDetailsAsync(id, _currentUserService.EmployeeId!.Value);
            if (report == null) return NotFound();

            return View(report);
        }

        // ============================================================
        // ✏️ CREATE OPERATIONS
        // ============================================================

        // ============ CREATE (with form) ============
        public async Task<IActionResult> Create()
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var employees = await _service.GetEmployeesForDropdownAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "Name");

            return View();
        }

        // ============ CREATE (POST) ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DamagedReport formModel, List<IFormFile> partIimages, List<IFormFile> partIIimages)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            ModelState.Remove("ControlNo"); // ControlNo is generated in the service, not user input

            // 1. Delegrate to Mapper to convert form model to DTO
            var request = formModel.ToCreateRequest(partIimages, partIIimages);

            // Handle Date Parsing (Moved from old controller logic)
            if (DateOnly.TryParseExact(Request.Form["DatePurchased"], "yyyy-MM-dd", out var dp))
                request.DatePurchased = dp;

            if (DateTime.TryParse(Request.Form["IncidentDateTime"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var idt))
                request.IncidentDateTime = idt;

            if (DateTime.TryParse(Request.Form["ReceivedDateTime"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var rdt))
                request.ReceivedDateTime = rdt;

            // 2. Delegate to Service (Service handles IsValidImage, Transactions, and DB saving)
            try
            {
                var created = await _service.CreateReportAsync(request, _currentUserService.EmployeeId!.Value);
                TempData["SuccessMessage"] = $"Damaged Report {created.ControlNo} created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message); // Catches image validation errors from service
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating damaged report for employee {EmployeeId}", _currentUserService.EmployeeId);
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            // Re-populate dropdown on error
            var employees = await _service.GetEmployeesForDropdownAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "Name");

            return View(formModel);
        }

        // ============================================================
        // 📝 UPDATE OPERATIONS
        // ============================================================

        // ============ EDIT (GET) ============
        public async Task<IActionResult> Edit(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            // Service checks ownership and returns null if not authorized
            var report = await _service.GetReportWithDetailsAsync(id, _currentUserService.EmployeeId!.Value);
            if (report == null) return NotFound();
            
            if (report.RequestStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Only reports with 'Draft' status can be edited.";
                return RedirectToAction(nameof(Index));
            }

            // Delegated to Service
            var employees = await _service.GetEmployeesForDropdownAsync();
            ViewBag.Employees = new SelectList(employees, "Id", "Name");

            return View(report);
        }

        // ============ EDIT (POST) ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            DamagedReport formModel,
            List<IFormFile> partIimages,
            List<IFormFile> partIIimages,
            List<int> deleteImageIds,
            List<DamagedReportFollowUp> formFollowUps)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            if (id != formModel.Id) return NotFound();

            // 1. Delegate to Mapper to convert form model + files + metadata to Update DTO
            var isGad = _currentUserService.EmployeeDepartment == "GAD";
            var request = formModel.ToUpdateRequest(partIimages, partIIimages, deleteImageIds, formFollowUps, isGad);

            // 2. Delegate to Service (Service handles atomic transaction for Report + FollowUps + Images)
            try
            {
                var success = await _service.UpdateReportAsync(request, _currentUserService.EmployeeId!.Value);
                if (!success) return NotFound();

                TempData["SuccessMessage"] = "Report updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred while updating.");
            }

            return View(formModel);
        
        }

        // ============================================================
        // 🗑️ DELETE OPERATIONS
        // ============================================================

        // ============ DELETE (GET) ============
        public async Task<IActionResult> Delete(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var report = await _service.GetReportWithDetailsAsync(id, _currentUserService.EmployeeId!.Value);
            if (report == null) return NotFound();

            if (report.RequestStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Only reports with 'Draft' status can be deleted.";
                return RedirectToAction(nameof(Index));
            }

            return View(report);
        }

        // ============ DELETE (POST) ============
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            try
            {   
                // 1 argument as the service handles storage deletion internally.
                var success = await _service.DeleteReportAsync(id);

                if (!success) return NotFound();

                TempData["SuccessMessage"] = "Report deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting damaged report {ReportId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the report.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // 🖨 PRINT OPERATION
        // ============================================================

        // ============ PRINT ============
        public async Task<IActionResult> Print(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var report = await _service.GetReportForPrintAsync(id);
            if (report == null) return NotFound();

            return View(report);
        }
    }
}