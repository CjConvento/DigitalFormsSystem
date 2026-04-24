using DigitalFormsSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalFormsSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly DigitalFormsSystemContext _context;

        public AccountController(DigitalFormsSystemContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.EmployeeNo == model.EmployeeNo);

                if (employee != null)
                {
                    // Hardcoded password (kung gusto mo)
                    if (model.Password != "hstpass")
                    {
                        ModelState.AddModelError("", "Invalid password.");
                        return View(model);
                    }

                    HttpContext.Session.SetInt32("EmployeeId", employee.Id);
                    HttpContext.Session.SetString("EmployeeName", employee.Name);
                    HttpContext.Session.SetString("EmployeeNo", employee.EmployeeNo);

                    return RedirectToAction("Index", "FixedAsset");
                }
                else
                {
                    ModelState.AddModelError("", "Employee not found.");
                }
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}