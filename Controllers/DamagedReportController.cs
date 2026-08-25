using DigitalFormsSystem.Core.Interfaces;
using DigitalFormsSystem.Core.Models;
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
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IAuditService _auditService;
        private readonly DigitalFormsSystemContext _context;  // <-- dinagdag

        public DamagedReportController(
            IDamagedReportService service,
            ICurrentUserService currentUserService,
            IAuditService auditService,
            IConfiguration config,
            IWebHostEnvironment env,
            DigitalFormsSystemContext context)  // <-- dinagdag sa constructor
        {
            _service = service;
            _currentUserService = currentUserService;
            _config = config;
            _auditService = auditService;
            _env = env;
            _context = context;  // <-- i-assign
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

            var report = await _service.GetReportWithDetailsAsync(id);
            if (report == null) return NotFound();
            return View(report);
        }

        // ============================================================
        // ✏️ CREATE OPERATIONS
        // ============================================================

        // ============ CREATE (GET) ============
        public IActionResult Create()
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name");
            return View();
        }

        // ============ CREATE (POST) ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DamagedReport report, List<IFormFile> partIimages, List<IFormFile> partIIimages)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var empId = _currentUserService.EmployeeId!.Value;
            report.ReportedByEmployeeId = empId;

            // Remove validation errors
            ModelState.Remove("ControlNo");
            ModelState.Remove("ReportedByEmployeeId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("RequestStatus");
            ModelState.Remove("ReportedByEmployee");
            ModelState.Remove("ReceivedByEmployee");
            ModelState.Remove("InvestigatedByEmployee");
            ModelState.Remove("VerifiedByEmployee");
            ModelState.Remove("NotedByEmployee");

            // Manual date parsing (copy from your existing code)
            var datePurchasedStr = Request.Form["DatePurchased"].ToString();
            if (!string.IsNullOrEmpty(datePurchasedStr))
            {
                if (!DateOnly.TryParseExact(datePurchasedStr, "yyyy-MM-dd", out var datePurchased))
                    ModelState.AddModelError("DatePurchased", "Invalid date format.");
                else
                    report.DatePurchased = datePurchased;
            }

            var incidentDateTimeStr = Request.Form["IncidentDateTime"].ToString();
            if (!string.IsNullOrWhiteSpace(incidentDateTimeStr))
            {
                var formats = new[] { "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff" };
                if (DateTime.TryParseExact(incidentDateTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var incidentDateTime))
                    report.IncidentDateTime = incidentDateTime;
                else if (DateTime.TryParse(incidentDateTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out incidentDateTime))
                    report.IncidentDateTime = incidentDateTime;
                else
                    ModelState.AddModelError("IncidentDateTime", "Invalid incident date/time format.");
            }

            var receivedDateTimeStr = Request.Form["ReceivedDateTime"].ToString();
            if (!string.IsNullOrWhiteSpace(receivedDateTimeStr))
            {
                var formats = new[] { "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff" };
                if (DateTime.TryParseExact(receivedDateTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var receivedDateTime))
                    report.ReceivedDateTime = receivedDateTime;
                else if (DateTime.TryParse(receivedDateTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out receivedDateTime))
                    report.ReceivedDateTime = receivedDateTime;
                else
                    ModelState.AddModelError("ReceivedDateTime", "Invalid received date/time format.");
            }

            // GAD-only: clear Part IV fields for non-GAD
            var isGad = _currentUserService.EmployeeDepartment == "GAD";
            if (!isGad)
            {
                report.Findings = null;
                report.Recommendation = null;
                report.NegligenceFlag = null;
                report.NegligenceDetails = null;
                report.Remarks = null;
                report.AdministrativeDiscipline = null;
                report.InvestigatedByEmployeeId = null;
                report.VerifiedByEmployeeId = null;
                report.NotedByEmployeeId = null;
            }

            // Validate images
            var maxFileSizeMB = _config.GetValue<int>("UploadSettings:MaxFileSizeMB", 5);
            bool hasImageError = false;

            if (partIimages != null)
            {
                foreach (var img in partIimages)
                {
                    if (img.Length > 0 && !_service.IsValidImage(img, out string errorMsg, maxFileSizeMB))
                    {
                        ModelState.AddModelError("partIimages", errorMsg);
                        hasImageError = true;
                    }
                }
            }

            if (partIIimages != null)
            {
                foreach (var img in partIIimages)
                {
                    if (img.Length > 0 && !_service.IsValidImage(img, out string errorMsg, maxFileSizeMB))
                    {
                        ModelState.AddModelError("partIIimages", errorMsg);
                        hasImageError = true;
                    }
                }
            }

            if (hasImageError)
            {
                ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", report.ReceivedByEmployeeId);
                return View(report);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var uploadsPath = _config["UploadSettings:DamagedReportsPath"] ?? "uploads/damagedreports";
                    var created = await _service.CreateReportAsync(
                        report,
                        partIimages,
                        partIIimages,
                        _env.WebRootPath,
                        uploadsPath,
                        maxFileSizeMB);

                    TempData["SuccessMessage"] = "Damaged Report created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating report: {ex.Message}");
                }
            }

            ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", report.ReceivedByEmployeeId);
            return View(report);
        }

        // ============================================================
        // 📝 UPDATE OPERATIONS
        // ============================================================

        // ============ EDIT (GET) ============
        public async Task<IActionResult> Edit(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            // ✅ Include FollowUps
            var report = await _context.DamagedReports
                .Include(r => r.Images)
                .Include(r => r.FollowUps)  // ✅ ADD THIS
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();

            if (report.RequestStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Only reports with 'Draft' status can be edited.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", report.ReceivedByEmployeeId);
            return View(report);
        }

        // ============ EDIT (POST) - Keep it simple for now ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, 
            DamagedReport updatedReport, 
            List<IFormFile> partIimages, 
            List<IFormFile> partIIimages, 
            List<int> deleteImageIds,
            List<DamagedReportFollowUp> FollowUps
)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            if (id != updatedReport.Id) return NotFound();

            // Remove validation
            ModelState.Remove("ControlNo");
            ModelState.Remove("ReportedByEmployeeId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("RequestStatus");
            ModelState.Remove("ReportedByEmployee");
            ModelState.Remove("ReceivedByEmployee");
            ModelState.Remove("InvestigatedByEmployee");
            ModelState.Remove("VerifiedByEmployee");
            ModelState.Remove("NotedByEmployee");

            // GAD check
            var isGad = _currentUserService.EmployeeDepartment == "GAD";
            if (!isGad)
            {
                updatedReport.Findings = null;
                updatedReport.Recommendation = null;
                updatedReport.NegligenceFlag = null;
                updatedReport.NegligenceDetails = null;
                updatedReport.Remarks = null;
                updatedReport.AdministrativeDiscipline = null;
                updatedReport.InvestigatedByEmployeeId = null;
                updatedReport.VerifiedByEmployeeId = null;
                updatedReport.NotedByEmployeeId = null;
            }

            // Validate new images
            var maxFileSizeMB = _config.GetValue<int>("UploadSettings:MaxFileSizeMB", 5);
            bool hasImageError = false;

            if (partIimages != null)
            {
                foreach (var img in partIimages)
                {
                    if (img.Length > 0 && !_service.IsValidImage(img, out string errorMsg, maxFileSizeMB))
                    {
                        ModelState.AddModelError("partIimages", errorMsg);
                        hasImageError = true;
                    }
                }
            }

            if (partIIimages != null)
            {
                foreach (var img in partIIimages)
                {
                    if (img.Length > 0 && !_service.IsValidImage(img, out string errorMsg, maxFileSizeMB))
                    {
                        ModelState.AddModelError("partIIimages", errorMsg);
                        hasImageError = true;
                    }
                }
            }

            if (hasImageError)
            {
                var originalReport = await _service.GetReportWithDetailsAsync(id);
                if (originalReport != null)
                {
                    // Copy scalar fields
                    originalReport.Item = updatedReport.Item;
                    originalReport.FixedAssetCode = updatedReport.FixedAssetCode;
                    originalReport.DatePurchased = updatedReport.DatePurchased;
                    originalReport.BrandSize = updatedReport.BrandSize;
                    originalReport.LocationUser = updatedReport.LocationUser;
                    originalReport.SerialNumber = updatedReport.SerialNumber;
                    originalReport.Color = updatedReport.Color;
                    originalReport.IncidentDateTime = updatedReport.IncidentDateTime;
                    originalReport.CauseOfDamage = updatedReport.CauseOfDamage;
                    originalReport.ImmediateAction = updatedReport.ImmediateAction;
                    originalReport.RecommendedAction = updatedReport.RecommendedAction;
                    originalReport.ReceivedByEmployeeId = updatedReport.ReceivedByEmployeeId;
                    originalReport.ReceivedDateTime = updatedReport.ReceivedDateTime;

                    if (isGad)
                    {
                        originalReport.Findings = updatedReport.Findings;
                        originalReport.Recommendation = updatedReport.Recommendation;
                        originalReport.NegligenceFlag = updatedReport.NegligenceFlag;
                        originalReport.NegligenceDetails = updatedReport.NegligenceDetails;
                        originalReport.Remarks = updatedReport.Remarks;
                        originalReport.AdministrativeDiscipline = updatedReport.AdministrativeDiscipline;
                        originalReport.InvestigatedByEmployeeId = updatedReport.InvestigatedByEmployeeId;
                        originalReport.VerifiedByEmployeeId = updatedReport.VerifiedByEmployeeId;
                        originalReport.NotedByEmployeeId = updatedReport.NotedByEmployeeId;
                    }

                    ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", originalReport.ReceivedByEmployeeId);
                    return View(originalReport);
                }

                ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", updatedReport.ReceivedByEmployeeId);
                return View(updatedReport);
            }

            if (ModelState.IsValid)
            {
                // ✅ START TRANSACTION
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // ============================================================
                    // 1. SAVE FOLLOW-UPS (PART IV)
                    // ============================================================
                    if (FollowUps != null)
                    {
                        // Remove existing follow-ups
                        var existingFollowUps = await _context.DamagedReportFollowUps
                            .Where(f => f.DamagedReportId == id)
                            .ToListAsync();
                        _context.DamagedReportFollowUps.RemoveRange(existingFollowUps);

                        // Add new follow-ups
                        foreach (var followUp in FollowUps)
                        {
                            if (followUp.FollowUpDate != default &&
                                !string.IsNullOrEmpty(followUp.Status))
                            {
                                followUp.DamagedReportId = id;
                                followUp.CreatedAt = DateTime.Now;
                                _context.DamagedReportFollowUps.Add(followUp);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    // ============================================================
                    // 2. UPDATE MAIN REPORT
                    // ============================================================
                    var uploadsPath = _config["UploadSettings:DamagedReportsPath"];
                    var success = await _service.UpdateReportAsync(
                        updatedReport,
                        partIimages,
                        partIIimages,
                        deleteImageIds,
                        _env.WebRootPath,
                        uploadsPath,
                        maxFileSizeMB);

                    if (!success)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Report not found or cannot be edited.";
                        return RedirectToAction(nameof(Index));
                    }

                    // ✅ COMMIT TRANSACTION (both succeeded)
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Report updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    if (!await _service.ReportExistsAsync(id)) return NotFound();
                    throw;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", updatedReport.ReceivedByEmployeeId);
            return View(updatedReport);
        }

        // ============================================================
        // 🗑️ DELETE OPERATIONS
        // ============================================================

        // ============ DELETE (GET) ============
        public async Task<IActionResult> Delete(int id)
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var report = await _service.GetReportWithDetailsAsync(id);
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
                var uploadsPath = _config["UploadSettings:DamagedReportsPath"];
                var success = await _service.DeleteReportAsync(id, _env.WebRootPath, uploadsPath);

                if (!success) return NotFound();

                TempData["SuccessMessage"] = "Report deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
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