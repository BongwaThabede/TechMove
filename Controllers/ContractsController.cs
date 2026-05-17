using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;

namespace TechMove.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly IFileValidationService _fileValidationService;
        private readonly IContractStatusService _contractStatusService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(
            ApplicationDbContext context,
            ICurrencyService currencyService,
            IFileValidationService fileValidationService,
            IContractStatusService contractStatusService,
            IWebHostEnvironment environment,
            ILogger<ContractsController> logger)
        {
            _context = context;
            _currencyService = currencyService;
            _fileValidationService = fileValidationService;
            _contractStatusService = contractStatusService;
            _environment = environment;
            _logger = logger;
        }

        // GET: Contracts
       // GET: Contracts with optional status filter
public async Task<IActionResult> Index(string? status)
{
    await _contractStatusService.SyncAllAsync(DateTime.UtcNow.Date);

    var query = _context.Contracts
        .Include(c => c.Client)
        .AsQueryable();

    // Filter by status if provided
    if (!string.IsNullOrEmpty(status) && status != "All")
    {
        query = query.Where(c => c.Status == status);
    }

    var contracts = await query
        .OrderByDescending(c => c.CreatedDate)
        .ToListAsync();
    
    return View(contracts);
}
        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (contract == null) return NotFound();

            if (_contractStatusService.SyncSingle(contract, DateTime.UtcNow.Date))
            {
                await _context.SaveChangesAsync();
            }

            return View(contract);
        }

        // GET: Contracts/Create
        [Authorize(Roles = "Admin,LogisticsManager")]
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name");
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,LogisticsManager")]
        public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,Status,ServiceLevel,ContractNumber,ContractValueUSD")] Contract contract, IFormFile? signedAgreement)
        {
            if (signedAgreement != null && signedAgreement.Length > 0)
            {
                if (!_fileValidationService.IsValidPdf(signedAgreement))
                {
                    ModelState.AddModelError("SignedAgreement", "Only PDF files are allowed.");
                    ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name", contract.ClientId);
                    return View(contract);
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "contracts");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(signedAgreement.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await signedAgreement.CopyToAsync(stream);
                }

                contract.SignedAgreementPath = $"/uploads/contracts/{uniqueFileName}";
                contract.SignedAgreementFileName = signedAgreement.FileName;
                contract.AgreementUploadDate = DateTime.UtcNow;
            }

            // Get exchange rate for USD to ZAR conversion
            var rate = await _currencyService.GetUSDToZARRateAsync();
            contract.ContractValueZAR = contract.ContractValueUSD * rate;
            contract.CreatedDate = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _context.Add(contract);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Contract created successfully!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null) return NotFound();
            
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel,ContractNumber,ContractValueUSD,SignedAgreementPath,SignedAgreementFileName")] Contract contract)
        {
            if (id != contract.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var rate = await _currencyService.GetUSDToZARRateAsync();
                    contract.ContractValueZAR = contract.ContractValueUSD * rate;
                    contract.LastModifiedDate = DateTime.UtcNow;
                    
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Contract updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Search
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, string status)
        {
            await _contractStatusService.SyncAllAsync(DateTime.UtcNow.Date);

            var query = _context.Contracts.Include(c => c.Client).AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(c => c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.EndDate <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(c => c.Status == status);
            }

            var contracts = await query.ToListAsync();
            ViewData["StatusFilter"] = new SelectList(new[] { "All", "Draft", "Active", "Expired", "On Hold" }, status ?? "All");
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            
            return View("Index", contracts);
        }

        // GET: Contracts/DownloadAgreement/5
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound();

            var webRootPath = _environment.WebRootPath ?? _environment.ContentRootPath;
            var filePath = Path.Combine(webRootPath, contract.SignedAgreementPath.TrimStart('/'));
            
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = contract.SignedAgreementFileName ?? $"contract_{contract.ContractNumber}.pdf";
            return File(fileBytes, "application/pdf", fileName);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Contract deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }
    }
}