using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechMove.Models;
using TechMove.Security;

namespace TechMove.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (HttpContext.IsLoggedIn())
            {
                return RedirectToAction(nameof(Dashboard));
            }

            return View(new LoginViewModel());
        }

        public IActionResult Dashboard()
        {
            if (!HttpContext.IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Username = HttpContext.GetCurrentUser();
            ViewBag.Role = HttpContext.GetCurrentRole();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
