using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;
using TechMove.Security;

namespace TechMove.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly IContractStatusService _contractStatusService;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            ApplicationDbContext context,
            ICurrencyService currencyService,
            IContractStatusService contractStatusService,
            ILogger<ServiceRequestsController> logger)
        {
            _context = context;
            _currencyService = currencyService;
            _contractStatusService = contractStatusService;
            _logger = logger;
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create(int? contractId)
        {
            if (!HttpContext.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!HttpContext.HasAnyRole("LogisticsManager", "Admin")) return Forbid();

            if (contractId == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(contractId);
            if (contract == null) return NotFound();

            if (_contractStatusService.SyncSingle(contract, DateTime.UtcNow.Date))
            {
                await _context.SaveChangesAsync();
            }

            // Workflow rule: only active contracts can raise requests.
            if (!contract.IsValidForServiceRequest(DateTime.UtcNow.Date))
            {
                TempData["ErrorMessage"] = $"Cannot create service request: Contract must be Active. Current status is {contract.Status}.";
                return RedirectToAction("Details", "Contracts", new { id = contractId });
            }

            // Get current exchange rate
            var rate = await _currencyService.GetUSDToZARRateAsync();
            ViewBag.CurrentRate = rate;
            ViewBag.ContractId = contractId;

            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,Description,Cost,Status")] ServiceRequest serviceRequest)
        {
            if (!HttpContext.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!HttpContext.HasAnyRole("LogisticsManager", "Admin")) return Forbid();

            // Validate contract status
            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
            if (contract != null && _contractStatusService.SyncSingle(contract, DateTime.UtcNow.Date))
            {
                await _context.SaveChangesAsync();
            }

            if (contract == null || !contract.IsValidForServiceRequest(DateTime.UtcNow.Date))
            {
                TempData["ErrorMessage"] = "Cannot create service request: only active contracts are allowed.";
                return RedirectToAction("Index", "Contracts");
            }

            // Get exchange rate and calculate ZAR cost
            var rate = await _currencyService.GetUSDToZARRateAsync();
            serviceRequest.CostInZAR = _currencyService.ConvertUSDToZAR(serviceRequest.Cost, rate);

            if (ModelState.IsValid)
            {
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CurrentRate = rate;
            ViewData["ContractId"] = new SelectList(_context.Contracts, "Id", "Id", serviceRequest.ContractId);
            return View(serviceRequest);
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            if (!HttpContext.IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!HttpContext.HasAnyRole("LogisticsManager", "Admin")) return Forbid();

            var serviceRequests = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
                .ToListAsync();
            return View(serviceRequests);
        }
    }
}