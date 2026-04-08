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
                .FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();
            return View(request);
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

            if (ModelState.IsValid)
            {
                request.RequestedByEmployeeId = empId.Value;
                request.RequestedByName = HttpContext.Session.GetString("EmployeeName");
                request.ControlNo = GenerateControlNo();
                request.CreatedAt = DateTime.Now;
                request.UpdatedAt = DateTime.Now;
                request.RequestStatus = "Draft";

                // ========== IDAGDAG ITO ==========
                request.RequestedAt = DateTime.Now;
                // ================================

                _context.Add(request);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
                .FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return NotFound();
            return View(request);
        }

        private string GenerateControlNo()
        {
            var year = DateTime.Now.ToString("yy");
            var month = DateTime.Now.ToString("MM");
            var last = _context.FixedAssetRequests
                .OrderByDescending(r => r.Id)
                .Select(r => r.ControlNo)
                .FirstOrDefault();
            int next = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var parts = last.Split('-');
                if (parts.Length >= 4 && int.TryParse(parts[3], out int num))
                    next = num + 1;
            }
            return $"GAD-FAR-{year}{month}-{next:D3}";
        }
    }
}