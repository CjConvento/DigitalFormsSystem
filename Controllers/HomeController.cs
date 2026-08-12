using Microsoft.AspNetCore.Mvc;
using DigitalFormsSystem.Core.Interfaces;

namespace DigitalFormsSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICurrentUserService _currentUserService;

        public HomeController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public IActionResult Index()
        {
            if (!_currentUserService.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}