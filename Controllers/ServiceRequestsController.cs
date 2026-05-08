using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;

namespace TechMove.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            ApplicationDbContext context,
            ICurrencyService currencyService,
            ILogger<ServiceRequestsController> logger)
        {
            _context = context;
            _currencyService = currencyService;
            _logger = logger;
        }

        // GET: ServiceRequests/Create
        public async Task<IActionResult> Create(int? contractId)
        {
            if (contractId == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(contractId);
            if (contract == null) return NotFound();

            // Check workflow rule
            if (contract.IsExpiredOrOnHold())
            {
                TempData["ErrorMessage"] = $"Cannot create service request: Contract is {contract.Status}";
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
            // Validate contract status
            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
            if (contract == null || contract.IsExpiredOrOnHold())
            {
                TempData["ErrorMessage"] = "Cannot create service request for expired or on-hold contracts";
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
            var serviceRequests = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
                .ToListAsync();
            return View(serviceRequests);
        }
    }
}