using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;  // ← ADD THIS LINE for ICurrencyService
using System.Security.Claims;

namespace TechMove.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ICurrencyService _currencyService;

        public DashboardController(
            ApplicationDbContext context, 
            IWebHostEnvironment environment,
            ICurrencyService currencyService)  // ← Now this will work
        {
            _context = context;
            _environment = environment;
            _currencyService = currencyService;
        }

        // Main Dashboard router
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(AdminDashboard));
            }
            else if (User.IsInRole("LogisticsManager"))
            {
                return RedirectToAction(nameof(ManagerDashboard));
            }
            else if (User.IsInRole("Finance"))
            {
                return RedirectToAction(nameof(FinanceDashboard));
            }
            else if (User.IsInRole("Client"))
            {
                return RedirectToAction(nameof(ClientDashboard));
            }
            
            return RedirectToAction("Index", "Home");
        }

        // ==================================================
        // 1. ADMIN DASHBOARD
        // ==================================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            // Stats Cards
            ViewBag.TotalClients = await _context.Clients.CountAsync();
            ViewBag.ActiveContracts = await _context.Contracts.CountAsync(c => c.Status == "Active");
            ViewBag.ExpiredContracts = await _context.Contracts.CountAsync(c => c.Status == "Expired");
            ViewBag.TotalRequests = await _context.ServiceRequests.CountAsync();
            ViewBag.PendingRequests = await _context.ServiceRequests.CountAsync(s => s.Status == "Pending");
            ViewBag.TotalContractValueZAR = await _context.Contracts.SumAsync(c => c.ContractValueZAR);

            // Recent Contracts
            ViewBag.RecentContracts = await _context.Contracts
                .Include(c => c.Client)
                .OrderByDescending(c => c.CreatedDate)
                .Take(10)
                .ToListAsync();

            // Recent Service Requests
            ViewBag.RecentRequests = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
                .OrderByDescending(s => s.CreatedDate)
                .Take(10)
                .ToListAsync();

            // All Clients for management
            ViewBag.Clients = await _context.Clients
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View();
        }

        // ==================================================
        // 2. LOGISTICS MANAGER DASHBOARD
        // ==================================================
        [Authorize(Roles = "LogisticsManager")]
        public async Task<IActionResult> ManagerDashboard()
        {
            // Get current exchange rate
            var rate = await _currencyService.GetUSDToZARRateAsync();
            ViewBag.CurrencyRate = rate;

            // Active Contracts only (for raising requests)
            var activeContracts = await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == "Active" && c.EndDate >= DateTime.UtcNow.Date)
                .OrderBy(c => c.EndDate)
                .ToListAsync();

            // Service requests (all for now)
            var myRequests = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            ViewBag.ActiveContracts = activeContracts;
            ViewBag.MyRequests = myRequests;
            ViewBag.TotalRequests = myRequests.Count;
            ViewBag.PendingRequests = myRequests.Count(r => r.Status == "Pending");

            return View();
        }

        // ==================================================
        // 3. FINANCE DASHBOARD
        // ==================================================
        [Authorize(Roles = "Finance")]
        public async Task<IActionResult> FinanceDashboard()
        {
            var totalRequests = await _context.ServiceRequests.CountAsync();
            var totalCostZAR = await _context.ServiceRequests.SumAsync(s => s.CostInZAR);
            var pendingRequests = await _context.ServiceRequests.CountAsync(s => s.Status == "Pending");
            var completedRequests = await _context.ServiceRequests.CountAsync(s => s.Status == "Completed");
            var avgCost = totalRequests > 0 ? totalCostZAR / totalRequests : 0;

            // Monthly breakdown
            var requestsByMonth = await _context.ServiceRequests
                .Where(s => s.RequestDate >= DateTime.UtcNow.AddMonths(-6))
                .GroupBy(s => new { s.RequestDate.Year, s.RequestDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Total = g.Sum(x => x.CostInZAR), Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var recentRequests = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
                .OrderByDescending(s => s.CreatedDate)
                .Take(20)
                .ToListAsync();

            ViewBag.TotalRequests = totalRequests;
            ViewBag.TotalCostZAR = totalCostZAR;
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.CompletedRequests = completedRequests;
            ViewBag.AvgCost = avgCost;
            ViewBag.RequestsByMonth = requestsByMonth;
            ViewBag.RecentRequests = recentRequests;

            return View();
        }

        // ==================================================
        // 4. CLIENT DASHBOARD
        // ==================================================
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> ClientDashboard()
        {
            // Get ClientId from claim
            var clientIdClaim = User.FindFirst("ClientId");
            if (clientIdClaim == null || !int.TryParse(clientIdClaim.Value, out int clientId))
            {
                TempData["ErrorMessage"] = "No client account linked. Please contact admin.";
                return RedirectToAction("Index", "Home");
            }

            var client = await _context.Clients.FindAsync(clientId);
            if (client == null)
            {
                TempData["ErrorMessage"] = "Client not found.";
                return RedirectToAction("Index", "Home");
            }

            // Get client's contracts
            var myContracts = await _context.Contracts
                .Include(c => c.ServiceRequests)
                .Where(c => c.ClientId == clientId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            // Get service requests for client's contracts
            var contractIds = myContracts.Select(c => c.Id).ToList();
            var myRequests = await _context.ServiceRequests
                .Where(s => contractIds.Contains(s.ContractId))
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            ViewBag.Client = client;
            ViewBag.MyContracts = myContracts;
            ViewBag.MyRequests = myRequests;
            ViewBag.TotalContracts = myContracts.Count;
            ViewBag.ActiveContracts = myContracts.Count(c => c.Status == "Active");
            ViewBag.TotalRequests = myRequests.Count;

            return View();
        }

        // ==================================================
        // PDF Download
        // ==================================================
        [HttpGet]
        public async Task<IActionResult> DownloadContractPdf(int contractId)
        {
            var contract = await _context.Contracts.FindAsync(contractId);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
            {
                TempData["ErrorMessage"] = "File not found.";
                return RedirectToAction("Index");
            }

            var webRootPath = _environment.WebRootPath ?? _environment.ContentRootPath;
            var fullPath = Path.Combine(webRootPath, contract.SignedAgreementPath.TrimStart('/'));
            
            if (!System.IO.File.Exists(fullPath))
            {
                // Try alternative path
                fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.SignedAgreementPath.TrimStart('/'));
            }
            
            if (!System.IO.File.Exists(fullPath))
            {
                TempData["ErrorMessage"] = "File not found on server.";
                return RedirectToAction("Index");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            var fileName = contract.SignedAgreementFileName ?? $"contract_{contract.ContractNumber}.pdf";
            return File(fileBytes, "application/pdf", fileName);
        }
    }
}