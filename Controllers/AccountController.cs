using DigitalFormsSystem.Models;
using DigitalFormsSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalFormsSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly DigitalFormsSystemContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AccountController(DigitalFormsSystemContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public IActionResult Login() => View();

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
                    if (model.Password != "hstpass")
                    {
                        ModelState.AddModelError("", "Invalid password.");
                        return View(model);
                    }

                    _currentUserService.SignIn(
                        employee.Id,
                        employee.Name,
                        employee.EmployeeNo,
                        employee.Department ?? ""
                    );

                    return RedirectToAction("Index", "Home");
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
            _currentUserService.SignOut();
            return RedirectToAction("Login");
        }
    }
}