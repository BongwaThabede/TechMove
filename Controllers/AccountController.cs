using Microsoft.AspNetCore.Mvc;
using TechMove.Models;
using TechMove.Security;

namespace TechMove.Controllers
{
    public class AccountController : Controller
    {
        // Demo users for assignment prototype.
        private static readonly Dictionary<string, (string Password, string Role)> Users = new(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = ("Admin@123", "Admin"),
            ["manager"] = ("Manager@123", "LogisticsManager"),
            ["user"] = ("User@123", "GeneralUser")
        };

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.IsLoggedIn())
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!Users.TryGetValue(model.Username, out var user) || user.Password != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            HttpContext.SignIn(model.Username, user.Role);
            return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.SignOutSession();
            return RedirectToAction("Index", "Home");
        }
    }
}
