using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TechMove.Data;
using TechMove.Models;

namespace TechMove.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Dashboard");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in: {Email}", model.Email);
                    return LocalRedirect(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // GET: Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View(new RegisterViewModel());
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Manual validation for checkboxes
            if (!model.AgreeTerms)
            {
                ModelState.AddModelError("AgreeTerms", "You must agree to the Terms & Conditions");
            }

            if (!model.AgreePrivacy)
            {
                ModelState.AddModelError("AgreePrivacy", "You must agree to the Privacy Policy");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                PhoneNumber = model.Phone
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Determine role based on email domain
                string role = DetermineUserRole(model.Email, model.AccountType);

                _logger.LogInformation("User {Email} assigned role: {Role}", model.Email, role);

                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(user, role);
                await _userManager.AddClaimAsync(user, new Claim("FirstName", model.FirstName));
                await _userManager.AddClaimAsync(user, new Claim("LastName", model.LastName));
                await _userManager.AddClaimAsync(user, new Claim("CompanyName", model.CompanyName));
                await _userManager.AddClaimAsync(user, new Claim("CompanyType", model.CompanyType));

                if (role == "Client")
                {
                    var client = new Client
                    {
                        Name = model.CompanyName,
                        ContactDetails = $"{model.FirstName} {model.LastName} - {model.Email} - {model.Phone}",
                        Region = "To be updated",
                        CreatedDate = DateTime.UtcNow
                    };
                    await _context.Clients.AddAsync(client);
                    await _context.SaveChangesAsync();
                    await _userManager.AddClaimAsync(user, new Claim("ClientId", client.Id.ToString()));
                }

                await _signInManager.SignInAsync(user, isPersistent: false);

                if (role == "Client")
                {
                    return RedirectToAction("ClientDashboard", "Dashboard");
                }

                return RedirectToAction("Index", "Dashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // Determine user role based on email domain
        private string DetermineUserRole(string email, string selectedAccountType)
        {
            string domain = email.Split('@').Last().ToLower();

            if (domain == "techmove.com")
            {
                if (!string.IsNullOrEmpty(selectedAccountType))
                {
                    return selectedAccountType;
                }
                return "LogisticsManager";
            }

            return "Client";
        }

        // POST: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        // GET: Access Denied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}