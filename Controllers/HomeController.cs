using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DigitalFormsSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Check if user is logged in
            var empId = HttpContext.Session.GetInt32("EmployeeId");
            if (empId == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}