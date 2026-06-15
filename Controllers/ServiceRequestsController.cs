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

        
        public async Task<IActionResult> Create(int? contractId)
        {
            
            if (!HttpContext.IsLoggedIn()) return RedirectToPage("/Account/Login", new { area = "Identity" });
            if (!HttpContext.HasAnyRole("LogisticsManager", "Admin")) return Forbid();

            if (contractId == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(contractId);
            if (contract == null) return NotFound();

            if (_contractStatusService.SyncSingle(contract, DateTime.UtcNow.Date))
            {
                await _context.SaveChangesAsync();
            }

            if (!contract.IsValidForServiceRequest(DateTime.UtcNow.Date))
            {
                TempData["ErrorMessage"] = $"Cannot create service request: Contract must be Active. Current status is {contract.Status}.";
                return RedirectToAction("ManagerDashboard", "Dashboard");
            }

            var rate = await _currencyService.GetUSDToZARRateAsync();
            ViewBag.CurrentRate = rate;
            ViewBag.ContractId = contractId;

            return View();
        }

        // POST: ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContractId,Description,Cost,Status,Priority")] ServiceRequest serviceRequest)
        {
            if (!HttpContext.IsLoggedIn()) return RedirectToPage("/Account/Login", new { area = "Identity" });
            if (!HttpContext.HasAnyRole("LogisticsManager", "Admin")) return Forbid();

            var contract = await _context.Contracts.FindAsync(serviceRequest.ContractId);
            if (contract != null && _contractStatusService.SyncSingle(contract, DateTime.UtcNow.Date))
            {
                await _context.SaveChangesAsync();
            }

            if (contract == null || !contract.IsValidForServiceRequest(DateTime.UtcNow.Date))
            {
                TempData["ErrorMessage"] = "Cannot create service request: only active contracts are allowed.";
                return RedirectToAction("ManagerDashboard", "Dashboard");
            }

            var rate = await _currencyService.GetUSDToZARRateAsync();
            serviceRequest.CostInZAR = _currencyService.ConvertUSDToZAR(serviceRequest.Cost, rate);
            serviceRequest.RequestNumber = $"REQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6)}";

            if (ModelState.IsValid)
            {
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Service request created successfully!";
                return RedirectToAction("ManagerDashboard", "Dashboard");
            }

            ViewBag.CurrentRate = rate;
            return View(serviceRequest);
        }

        // GET: ServiceRequests
        public async Task<IActionResult> Index()
        {
            if (!HttpContext.IsLoggedIn()) return RedirectToPage("/Account/Login", new { area = "Identity" });
            if (!HttpContext.HasAnyRole("LogisticsManager", "Admin", "Finance")) return Forbid();

            var serviceRequests = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c!.Client)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
            return View(serviceRequests);
        }
    }
}