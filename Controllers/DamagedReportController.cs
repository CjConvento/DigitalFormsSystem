using DigitalFormsSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

            // Remove validation errors for properties we set manually
            ModelState.Remove("ControlNo");
            ModelState.Remove("ReportedByEmployeeId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");
            ModelState.Remove("RequestStatus");

            // Remove validation for navigation properties
            ModelState.Remove("ReportedByEmployee");
            ModelState.Remove("ReceivedByEmployee");
            ModelState.Remove("InvestigatedByEmployee");
            ModelState.Remove("VerifiedByEmployee");
            ModelState.Remove("NotedByEmployee");

            // ========== MANUAL DATE PARSING ==========
            // DatePurchased (DateOnly?)
            var datePurchasedStr = Request.Form["DatePurchased"].ToString();
            if (!string.IsNullOrEmpty(datePurchasedStr))
            {
                if (!DateOnly.TryParseExact(datePurchasedStr, "yyyy-MM-dd", out var datePurchased))
                    ModelState.AddModelError("DatePurchased", "Invalid date format.");
                else
                    report.DatePurchased = datePurchased;
            }
            else
            {
                report.DatePurchased = null;
            }

            // IncidentDateTime (DateTime?)
            var incidentDateTimeStr = Request.Form["IncidentDateTime"].ToString();
            // Log the raw value to the Output window (View → Output in VS)
            Console.WriteLine($"IncidentDateTime raw: '{incidentDateTimeStr}'");

            if (!string.IsNullOrWhiteSpace(incidentDateTimeStr))
            {
                // Try multiple common formats
                var formats = new[] { "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff" };
                if (DateTime.TryParseExact(incidentDateTimeStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var incidentDateTime))
                    report.IncidentDateTime = incidentDateTime;
                else if (DateTime.TryParse(incidentDateTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out incidentDateTime))
                    report.IncidentDateTime = incidentDateTime;
                else
                    ModelState.AddModelError("IncidentDateTime", "Invalid incident date/time format.");
            }
            else
            {
                report.IncidentDateTime = null;
            }

            // ReceivedDateTime (same)
            var receivedDateTimeStr = Request.Form["ReceivedDateTime"].ToString();
            Console.WriteLine($"ReceivedDateTime raw: '{receivedDateTimeStr}'");

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
            else
            {
                report.ReceivedDateTime = null;
            }
            // ========================================

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

            // === 1. Validate all images before saving ===
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

                // Save Part I images
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

                // Save Part II images
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

            // Preserve existing units (images are not preserved, but that's acceptable)
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
                .Include(r => r.ReportedByEmployee)
                .Include(r => r.ReceivedByEmployee)
                .Include(r => r.Images)  // ✅ siguradong kasama ang mga larawan
                .AsNoTracking()          // optional: basahin lang, hindi na kailangan i-save
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();

            // Siguraduhing ang Images ay hindi null (kung walang laman, gawing empty list)
            if (report.Images == null)
                report.Images = new List<DamagedReportImage>();

            return View(report);
        }


        //#HELPERS

        private string GenerateControlNo(int retryCount = 0)
        {
            if (retryCount > 5)
                throw new Exception("Unable to generate a unique control number after 5 attempts.");

            var year = DateTime.Now.ToString("yy");
            var month = DateTime.Now.ToString("MM");
            var prefix = $"GAD-DR-{year}{month}-";

            var lastRequest = _context.DamagedReports
                .Where(r => r.ControlNo != null && r.ControlNo.StartsWith(prefix))
                .OrderByDescending(r => r.ControlNo)
                .Select(r => r.ControlNo)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastRequest != null && lastRequest.Length > prefix.Length)
            {
                if (int.TryParse(lastRequest.Substring(prefix.Length), out int lastNum))
                    nextNumber = lastNum + 1;
            }

            string newControlNo = $"{prefix}{nextNumber:D3}";

            // Avoid race condition: check again if the generated number already exists
            bool alreadyExists = _context.DamagedReports.Any(r => r.ControlNo == newControlNo);
            if (alreadyExists)
            {
                return GenerateControlNo(retryCount + 1);
            }

            return newControlNo;
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