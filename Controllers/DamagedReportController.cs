using DigitalFormsSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalFormsSystem.Controllers
{
    public class DamagedReportController : Controller
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public DamagedReportController(DigitalFormsSystemContext context, IWebHostEnvironment env, IConfiguration config)
        {
            _context = context;
            _env = env;
            _config = config;
        }

        // GET: DamagedReport
        public async Task<IActionResult> Index()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var reports = await _context.DamagedReports
                .Include(r => r.ReportedByEmployee)
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
                .Include(r => r.ReportedByEmployee)
                .Include(r => r.ReceivedByEmployee)
                .Include(r => r.Images) // include images
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
        public async Task<IActionResult> Create(DamagedReport report, List<IFormFile> partIimages, List<IFormFile> partIIimages)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            // Auto-set fields (but don't save yet)
            report.ReportedByEmployeeId = empId.Value;
            report.ControlNo = GenerateControlNo();
            report.CreatedAt = DateTime.Now;
            report.UpdatedAt = DateTime.Now;
            report.RequestStatus = "Draft";

            ModelState.Remove("ReportedByEmployee");
            ModelState.Remove("ReceivedByEmployee");
            ModelState.Remove("InvestigatedByEmployee");
            ModelState.Remove("VerifiedByEmployee");
            ModelState.Remove("NotedByEmployee");

            // GAD-only: clear Part IV fields for non-GAD
            var isGad = HttpContext.Session.GetString("EmployeeDepartment") == "GAD";
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

            // === 1. Validate all images before saving the report ===
            bool hasImageError = false;

            if (partIimages != null)
            {
                foreach (var img in partIimages)
                {
                    if (img.Length > 0 && !IsValidImage(img, out string errorMsg))
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
                    if (img.Length > 0 && !IsValidImage(img, out string errorMsg))
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

            // === 2. If all images are valid, save the report ===
            if (ModelState.IsValid)
            {
                _context.Add(report);
                await _context.SaveChangesAsync();

                // --- Save Part I images ---
                var uploadsFolder = GetUploadsFolder();
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                if (partIimages != null)
                {
                    foreach (var img in partIimages)
                    {
                        if (img.Length > 0)
                        {
                            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                            var filePath = Path.Combine(uploadsFolder, uniqueName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await img.CopyToAsync(stream);
                            }
                            var imageRecord = new DamagedReportImage
                            {
                                DamagedReportId = report.Id,
                                Section = "PartI",
                                FileName = img.FileName,
                                FilePath = $"/{_config["UploadSettings:DamagedReportsPath"]}/{uniqueName}",
                                ContentType = img.ContentType,
                                UploadedAt = DateTime.Now
                            };
                            _context.DamagedReportImages.Add(imageRecord);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // --- Save Part II images ---
                if (partIIimages != null)
                {
                    foreach (var img in partIIimages)
                    {
                        if (img.Length > 0)
                        {
                            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                            var filePath = Path.Combine(uploadsFolder, uniqueName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await img.CopyToAsync(stream);
                            }
                            var imageRecord = new DamagedReportImage
                            {
                                DamagedReportId = report.Id,
                                Section = "PartII",
                                FileName = img.FileName,
                                FilePath = $"/{_config["UploadSettings:DamagedReportsPath"]}/{uniqueName}",
                                ContentType = img.ContentType,
                                UploadedAt = DateTime.Now
                            };
                            _context.DamagedReportImages.Add(imageRecord);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Damaged Report created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", report.ReceivedByEmployeeId);
            return View(report);
        }

        // GET: DamagedReport/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var report = await _context.DamagedReports
                .Include(r => r.Images)
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

        // POST: DamagedReport/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DamagedReport updatedReport, List<IFormFile> partIimages, List<IFormFile> partIIimages, List<int> deleteImageIds)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            if (id != updatedReport.Id) return NotFound();

            ModelState.Remove("ReportedByEmployee");
            ModelState.Remove("ReceivedByEmployee");
            ModelState.Remove("InvestigatedByEmployee");
            ModelState.Remove("VerifiedByEmployee");
            ModelState.Remove("NotedByEmployee");

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.DamagedReports
                        .Include(r => r.Images)
                        .FirstOrDefaultAsync(r => r.Id == id);
                    if (existing == null) return NotFound();

                    if (existing.RequestStatus != "Draft")
                    {
                        TempData["ErrorMessage"] = "Only reports with 'Draft' status can be edited.";
                        return RedirectToAction(nameof(Index));
                    }

                    // --- 1. Delete selected images (works for both sections) ---
                    if (deleteImageIds != null && deleteImageIds.Any())
                    {
                        var imagesToDelete = existing.Images.Where(i => deleteImageIds.Contains(i.Id)).ToList();
                        foreach (var img in imagesToDelete)
                        {
                            var fullPath = Path.Combine(GetUploadsFolder(), Path.GetFileName(img.FilePath));
                            if (System.IO.File.Exists(fullPath))
                                System.IO.File.Delete(fullPath);
                            _context.DamagedReportImages.Remove(img);
                        }
                        await _context.SaveChangesAsync();
                    }

                    // --- 2. Update scalar fields ---
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
                    existing.UpdatedAt = DateTime.Now;

                    var isGad = HttpContext.Session.GetString("EmployeeDepartment") == "GAD";
                    if (isGad)
                    {
                        existing.Findings = updatedReport.Findings;
                        existing.Recommendation = updatedReport.Recommendation;
                        existing.NegligenceFlag = updatedReport.NegligenceFlag;
                        existing.NegligenceDetails = updatedReport.NegligenceDetails;
                        existing.Remarks = updatedReport.Remarks;
                        existing.AdministrativeDiscipline = updatedReport.AdministrativeDiscipline;
                        existing.InvestigatedByEmployeeId = updatedReport.InvestigatedByEmployeeId;
                        existing.VerifiedByEmployeeId = updatedReport.VerifiedByEmployeeId;
                        existing.NotedByEmployeeId = updatedReport.NotedByEmployeeId;
                    }

                    await _context.SaveChangesAsync();   // <-- nandito na yung save ng scalar changes

                    // ========== ILAGAY DITO ANG VALIDATION NG MGA BAGONG IMAGES ==========
                    bool hasImageError = false;

                    if (partIimages != null)
                    {
                        foreach (var img in partIimages)
                        {
                            if (img.Length > 0 && !IsValidImage(img, out string errorMsg))
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
                            if (img.Length > 0 && !IsValidImage(img, out string errorMsg))
                            {
                                ModelState.AddModelError("partIIimages", errorMsg);
                                hasImageError = true;
                            }
                        }
                    }
                    if (hasImageError)
                    {
                        ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", updatedReport.ReceivedByEmployeeId);
                        return View(updatedReport);
                    }
                    // ====================================================================

                    // --- 3. Upload folder (ensure exists) ---
                    var uploadsFolder = GetUploadsFolder();
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // --- 4. Add new Part I images (with validation) ---
                    if (partIimages != null && partIimages.Any())
                    {
                        foreach (var img in partIimages)
                        {
                            if (img.Length > 0)
                            {
                                var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                                var filePath = Path.Combine(uploadsFolder, uniqueName);
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await img.CopyToAsync(stream);
                                }
                                var imageRecord = new DamagedReportImage
                                {
                                    DamagedReportId = existing.Id,
                                    Section = "PartI",
                                    FileName = img.FileName,
                                    FilePath = $"/{_config["UploadSettings:DamagedReportsPath"]}/{uniqueName}",
                                    ContentType = img.ContentType,
                                    UploadedAt = DateTime.Now
                                };
                                _context.DamagedReportImages.Add(imageRecord);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    // --- 5. Add new Part II images (with validation) ---
                    if (partIIimages != null && partIIimages.Any())
                    {
                        foreach (var img in partIIimages)
                        {
                            if (img.Length > 0)
                            {
                                var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                                var filePath = Path.Combine(uploadsFolder, uniqueName);
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await img.CopyToAsync(stream);
                                }
                                var imageRecord = new DamagedReportImage
                                {
                                    DamagedReportId = existing.Id,
                                    Section = "PartII",
                                    FileName = img.FileName,
                                    FilePath = $"/{_config["UploadSettings:DamagedReportsPath"]}/{uniqueName}",
                                    ContentType = img.ContentType,
                                    UploadedAt = DateTime.Now
                                };
                                _context.DamagedReportImages.Add(imageRecord);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Report updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DamagedReports.Any(e => e.Id == id)) return NotFound();
                    throw;
                }
            }

            ViewBag.Employees = new SelectList(_context.Employees, "Id", "Name", updatedReport.ReceivedByEmployeeId);
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

        // POST: DamagedReport/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var report = await _context.DamagedReports
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

            // Delete physical image files
            var uploadsFolder = GetUploadsFolder();
            foreach (var img in report.Images)
            {
                var fullPath = Path.Combine(uploadsFolder, Path.GetFileName(img.FilePath));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

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
                .Include(r => r.ReportedByEmployee)   // fixed: was ReportedByEmployeeId
                .Include(r => r.ReceivedByEmployee)
                .Include(r => r.Images)              // include images for print (optional)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();
            return View(report);
        }


        //#HELPERS

        private string GenerateControlNo()
        {
            var year = DateTime.Now.ToString("yy");
            var month = DateTime.Now.ToString("MM");
            var prefix = $"GAD-DR-{year}{month}-";
            var last = _context.DamagedReports
                .Where(r => r.ControlNo != null && r.ControlNo.StartsWith(prefix))
                .OrderByDescending(r => r.ControlNo)
                .Select(r => r.ControlNo)
                .FirstOrDefault();
            int next = 1;
            if (last != null && last.Length > prefix.Length)
            {
                if (int.TryParse(last.Substring(prefix.Length), out int lastNum))
                    next = lastNum + 1;
            }
            return $"{prefix}{next:D3}";
        }

        private bool IsValidImage(IFormFile file, out string errorMessage)
        {
            errorMessage = null;
            // Read max file size from configuration; default to 5 MB if not set
            var maxSizeMB = _config.GetValue<int>("UploadSettings:MaxFileSizeMB", 5);
            if (file.Length > maxSizeMB * 1024 * 1024)
            {
                errorMessage = $"File {file.FileName} exceeds {maxSizeMB} MB limit.";
                return false;
            }
            // Allowed extensions
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                errorMessage = $"File {file.FileName} has an invalid extension. Allowed: {string.Join(", ", allowedExtensions)}";
                return false;
            }
            // Allowed content types
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                errorMessage = $"File {file.FileName} has an invalid content type.";
                return false;
            }
            return true;
        }

        private string GetUploadsFolder()
        {
            var relativePath = _config["UploadSettings:DamagedReportsPath"];
            return Path.Combine(_env.WebRootPath, relativePath);
        }
    }
}