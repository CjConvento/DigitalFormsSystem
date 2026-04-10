using FixedAssetSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FixedAssetSystem.Controllers
{
    public class FixedAssetController : Controller
    {
        private readonly FixedAssetSystemContext _context;

        public FixedAssetController(FixedAssetSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var requests = await _context.FixedAssetRequests
                .Include(r => r.RequestedByEmployee)
                .OrderByDescending(r => r.DateRequested)
                .ToListAsync();
            return View(requests);
        }

        public async Task<IActionResult> Details(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var request = await _context.FixedAssetRequests
                .Include(r => r.RequestedByEmployee)
                .Include(r => r.EvaluatedByEmployee)
                .Include(r => r.ExistingUnitDetails)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();
            return View(request);
        }

        // GET: FixedAsset/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var request = await _context.FixedAssetRequests
                .Include(r => r.ExistingUnitDetails)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            // Optional: only allow editing of "Draft" requests
            if (request.RequestStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Only requests with 'Draft' status can be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(request);
        }

        // POST: FixedAsset/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FixedAssetRequest updatedRequest)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            if (id != updatedRequest.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.FixedAssetRequests
                        .Include(r => r.ExistingUnitDetails)
                        .FirstOrDefaultAsync(r => r.Id == id);

                    if (existing == null) return NotFound();

                    // Update basic fields (do not change ControlNo, RequestedBy, CreatedAt)
                    existing.Department = updatedRequest.Department;
                    existing.Section = updatedRequest.Section;
                    existing.TargetDateNeeded = updatedRequest.TargetDateNeeded;
                    existing.Quantity = updatedRequest.Quantity;
                    existing.AssetType = updatedRequest.AssetType;
                    existing.DetailedDescription = updatedRequest.DetailedDescription;
                    existing.ReasonPurpose = updatedRequest.ReasonPurpose;
                    existing.ProposedLocation = updatedRequest.ProposedLocation;
                    existing.EstimatedLifeSpan = updatedRequest.EstimatedLifeSpan;
                    existing.RequestType = updatedRequest.RequestType;
                    existing.DamagedReportNo = updatedRequest.DamagedReportNo;
                    existing.EvaluatedByName = updatedRequest.EvaluatedByName;
                    existing.UpdatedAt = DateTime.Now;

                    // Handle ExistingUnitDetails for "Additional" requests
                    if (existing.RequestType == "Additional")
                    {
                        // Remove old existing units
                        _context.ExistingUnitDetails.RemoveRange(existing.ExistingUnitDetails);

                        // Add new ones from the form
                        var itemNos = Request.Form["ExistingUnits[].ItemNo"];
                        var descriptions = Request.Form["ExistingUnits[].Description"];
                        var locations = Request.Form["ExistingUnits[].Location"];
                        var userNames = Request.Form["ExistingUnits[].UserName"];
                        var remarks = Request.Form["ExistingUnits[].Remarks"];

                        for (int i = 0; i < descriptions.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(descriptions[i]))
                            {
                                var unit = new ExistingUnitDetail
                                {
                                    FixedAssetRequestId = existing.Id,
                                    ItemNo = int.TryParse(itemNos[i], out int ino) ? ino : i + 1,
                                    Description = descriptions[i]!,
                                    Location = locations[i] ?? string.Empty,
                                    UserName = userNames[i] ?? string.Empty,
                                    Remarks = remarks[i] ?? string.Empty
                                };
                                _context.ExistingUnitDetails.Add(unit);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Request updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.FixedAssetRequests.Any(e => e.Id == id)) return NotFound();
                    throw;
                }
            }
            return View(updatedRequest);
        }

        // GET: FixedAsset/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var request = await _context.FixedAssetRequests
                .Include(r => r.ExistingUnitDetails)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            // Optional: only allow deletion of "Draft" requests
            if (request.RequestStatus != "Draft")
            {
                TempData["ErrorMessage"] = "Only requests with 'Draft' status can be deleted.";
                return RedirectToAction(nameof(Index));
            }

            return View(request);
        }

        // POST: FixedAsset/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            var request = await _context.FixedAssetRequests
                .Include(r => r.ExistingUnitDetails)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null) return NotFound();

            // Remove existing units first (if any)
            if (request.ExistingUnitDetails != null && request.ExistingUnitDetails.Any())
            {
                _context.ExistingUnitDetails.RemoveRange(request.ExistingUnitDetails);
            }

            // Remove the main request
            _context.FixedAssetRequests.Remove(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Request deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FixedAssetRequest request)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            // Set foreign key and name BEFORE validation
            request.RequestedByEmployeeId = empId.Value;
            request.RequestedByName = HttpContext.Session.GetString("EmployeeName");

            // Manual parsing of TargetDateNeeded from string (because input is type="text")
            var targetDateStr = Request.Form["TargetDateNeeded"].ToString();
            if (!DateOnly.TryParseExact(targetDateStr, "yyyy-MM-dd", out var targetDate))
            {
                ModelState.AddModelError("TargetDateNeeded", "Invalid date format. Please use YYYY-MM-DD.");
                return View(request);
            }
            request.TargetDateNeeded = targetDate;

            // Remove validation for navigation properties (they are not needed for creation)
            ModelState.Remove("RequestedByEmployee");
            ModelState.Remove("EvaluatedByEmployee");

            if (ModelState.IsValid)
            {
                // Set fixed fields (these do not change on retry)
                request.RequestedByEmployeeId = empId.Value;
                request.RequestedByName = HttpContext.Session.GetString("EmployeeName");
                request.CreatedAt = DateTime.Now;
                request.UpdatedAt = DateTime.Now;
                request.RequestStatus = "Draft";
                request.RequestedAt = DateTime.Now;
                request.DateRequested = DateOnly.FromDateTime(DateTime.Now);   // <-- ADD THIS

                int maxRetries = 3;
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        // Generate control number (on retry, generate a new one)
                        if (attempt > 1)
                            request.ControlNo = GenerateControlNo(empId.Value, attempt - 1);
                        else
                            request.ControlNo = GenerateControlNo(empId.Value);

                        _context.Add(request);
                        await _context.SaveChangesAsync();

                        // Save existing units if request type is "Additional"
                        if (request.RequestType == "Additional")
                        {
                            var itemNos = Request.Form["ExistingUnits[].ItemNo"];
                            var descriptions = Request.Form["ExistingUnits[].Description"];
                            var locations = Request.Form["ExistingUnits[].Location"];
                            var userNames = Request.Form["ExistingUnits[].UserName"];
                            var remarks = Request.Form["ExistingUnits[].Remarks"];

                            for (int i = 0; i < descriptions.Count; i++)
                            {
                                string? desc = descriptions[i];
                                if (!string.IsNullOrEmpty(desc))
                                {
                                    var unit = new ExistingUnitDetail
                                    {
                                        FixedAssetRequestId = request.Id,
                                        ItemNo = int.TryParse(itemNos[i], out int ino) ? ino : i + 1,
                                        Description = desc,
                                        Location = locations[i] ?? string.Empty,
                                        UserName = userNames[i] ?? string.Empty,
                                        Remarks = remarks[i] ?? string.Empty
                                    };
                                    _context.ExistingUnitDetails.Add(unit);
                                }
                            }
                            await _context.SaveChangesAsync();
                        }

                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UQ_FixedAssetRequests_ControlNo") == true)
                    {
                        if (attempt == maxRetries)
                            throw; // rethrow if all retries fail
                                   // Otherwise, loop again – a new control number will be generated
                    }
                }
            }
            return View(request);
        }

        public async Task<IActionResult> Print(int id)
        {
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null) return RedirectToAction("Login", "Account");

            // Optional: stored procedure logging
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_PrintFixedAssetRequest @RequestID={0}, @PrintedByEmployeeID={1}",
                    id, empId.Value);
            }
            catch { }

            var request = await _context.FixedAssetRequests
                .Include(r => r.RequestedByEmployee)
                .Include(r => r.EvaluatedByEmployee)
                    .Include(r => r.ExistingUnitDetails)   // <--- IDAGDAG ITO
                .FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();
            return View(request);
        }

        private string GenerateControlNo(int employeeId, int retryCount = 0)
        {
            if (retryCount > 5)
                throw new Exception("Unable to generate a unique control number after 5 attempts.");

            var employee = _context.Employees.Find(employeeId);
            string location = employee?.Location?.Trim() ?? "F1";
            string year = DateTime.Now.ToString("yy");
            string prefix = $"GAD-FAR-{location}-{year}-";

            // Kunin ang pinakamataas na umiiral na numero para sa prefix na ito
            var lastRequest = _context.FixedAssetRequests
                .Where(r => r.ControlNo != null && r.ControlNo.StartsWith(prefix))
                .OrderByDescending(r => r.ControlNo)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastRequest != null)
            {
                string lastNumberStr = lastRequest.ControlNo.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNum))
                    nextNumber = lastNum + 1;
            }

            string newControlNo = $"{prefix}{nextNumber:D3}";

            // Bago i‑return, tiyakin na wala pang gumagamit ng numerong ito (para iwasan ang bihirang race condition)
            bool alreadyExists = _context.FixedAssetRequests.Any(r => r.ControlNo == newControlNo);
            if (alreadyExists)
            {
                // Kung may umiiral na (napakabihirang), mag‑retry – tataas ang nextNumber sa susunod na tawag
                return GenerateControlNo(employeeId, retryCount + 1);
            }

            return newControlNo;
        }
    }
}