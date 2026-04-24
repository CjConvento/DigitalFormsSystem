using DigitalFormsSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DigitalFormsSystem.Controllers
{
    public class DamagedReportController : Controller
    {
        private readonly DigitalFormsSystemContext _context;

        public DamagedReportController(DigitalFormsSystemContext context)
        {
            _context = context;
        }

        // GET: DamagedReport
        public async Task<IActionResult> Index()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var reports = await _context.DamagedReports
                .Include(r => r.ReportedByEmployeeId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reports);
        }

        // GET: DamagedReport/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var report = await _context.DamagedReports
                .Include(r => r.ReportedByEmployeeId)
                .Include(r => r.ReportedByEmployeeId)
                .Include(r => r.InvestigatedByEmployeeId)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();
            return View(report);
        }

        // GET: DamagedReport/Create
        public IActionResult Create()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name");
            return View();
        }

        // POST: DamagedReport/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DamagedReport report)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                report.ControlNo = GenerateControlNo();
                report.ReportedByEmployeeId = empId.Value;
                report.CreatedAt = DateTime.Now;
                report.UpdatedAt = DateTime.Now;
                report.RequestStatus = "Draft";
                _context.Add(report);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", report.ReportedByEmployeeId);
            return View(report);
        }

        // GET: DamagedReport/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var report = await _context.DamagedReports.FindAsync(id);
            if (report == null) return NotFound();
            if (report.RequestStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Only reports with 'Draft' status can be edited.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", report.ReportedByEmployeeId);
            return View(report);
        }

        // POST: DamagedReport/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DamagedReport updatedReport)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            if (id != updatedReport.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.DamagedReports.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Item = updatedReport.Item;
                    existing.FixedAssetCode = updatedReport.FixedAssetCode;
                    existing.DatePurchased = updatedReport.DatePurchased;
                    existing.BrandSize = updatedReport.BrandSize;
                    existing.LocationUser = updatedReport.LocationUser;
                    existing.SerialNumber = updatedReport.SerialNumber;
                    existing.Color = updatedReport.Color;
                    existing.IncidentDateTime = updatedReport.IncidentDateTime;
                    existing.CauseOfDamage = updatedReport.CauseOfDamage;
                    existing.ImmediateAction = updatedReport.ImmediateAction;
                    existing.RecommendedAction = updatedReport.RecommendedAction;
                    existing.ReceivedByEmployeeId = updatedReport.ReceivedByEmployeeId;
                    existing.ReceivedDateTime = updatedReport.ReceivedDateTime;
                    existing.Findings = updatedReport.Findings;
                    existing.Recommendation = updatedReport.Recommendation;
                    existing.NegligenceFlag = updatedReport.NegligenceFlag;
                    existing.NegligenceDetails = updatedReport.NegligenceDetails;
                    existing.Remarks = updatedReport.Remarks;
                    existing.AdministrativeDiscipline = updatedReport.AdministrativeDiscipline;
                    existing.InvestigatedByEmployeeId = updatedReport.InvestigatedByEmployeeId;
                    existing.VerifiedByEmployeeId = updatedReport.VerifiedByEmployeeId;
                    existing.NotedByEmployeeId = updatedReport.NotedByEmployeeId;
                    existing.RequestStatus = updatedReport.RequestStatus;
                    existing.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Report updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DamagedReports.Any(e => e.Id == id)) return NotFound();
                    throw;
                }
            }
            ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", updatedReport.ReportedByEmployeeId);
            return View(updatedReport);
        }

        // GET: DamagedReport/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var report = await _context.DamagedReports.FindAsync(id);
            if (report == null) return NotFound();
            if (report.RequestStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Only reports with 'Draft' status can be deleted.";
                return RedirectToAction(nameof(Index));
            }
            return View(report);
        }

        // POST: DamagedReport/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var report = await _context.DamagedReports.FindAsync(id);
            if (report == null) return NotFound();
            _context.DamagedReports.Remove(report);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Report deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: DamagedReport/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var report = await _context.DamagedReports
                .Include(r => r.ReportedByEmployeeId)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();
            return View(report);
        }

        private string GenerateControlNo()
        {
            var year = DateTime.Now.ToString("yy");
            var month = DateTime.Now.ToString("MM");
            var count = _context.DamagedReports.Count() + 1;
            return $"GAD-DR-{year}{month}-{count:D3}";
        }
    }
}