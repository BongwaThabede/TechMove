using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using TechMove.Models;
using TechMove.Security;

namespace TechMove.Controllers
{
    public class AccountController : Controller
    {
        private static readonly ConcurrentDictionary<string, (string Password, string Role)> Users = new(
            new Dictionary<string, (string Password, string Role)>(StringComparer.OrdinalIgnoreCase)
            {
                ["admin"] = ("Admin@123", "Admin"),
                ["manager"] = ("Manager@123", "LogisticsManager"),
                ["user"] = ("User@123", "GeneralUser")
            },
            StringComparer.OrdinalIgnoreCase);

        private static readonly ConcurrentDictionary<string, RegisteredUserProfile> RegisteredProfiles = new(StringComparer.OrdinalIgnoreCase);

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

            var key = model.Username.Trim();
            if (!Users.TryGetValue(key, out var user) || user.Password != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            HttpContext.SignIn(key, user.Role);
            return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.IsLoggedIn())
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var key = model.Email.Trim();
            if (Users.ContainsKey(key))
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            if (!Users.TryAdd(key, (model.Password, model.AccountType)))
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            RegisteredProfiles[key] = new RegisteredUserProfile
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Phone = model.Phone.Trim(),
                CompanyName = model.CompanyName.Trim(),
                CompanyType = model.CompanyType
            };

            TempData["AccountCreated"] = "1";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (HttpContext.IsLoggedIn())
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["ForgotPasswordRequested"] = "1";
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            if (TempData.Peek("ForgotPasswordRequested") as string != "1")
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            return View();
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
