using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using TechMove.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace TechMove.Controllers
{
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
        public async Task<IActionResult> Index()
        {
            await _contractStatusService.SyncAllAsync(DateTime.UtcNow.Date);

            var contracts = await _context.Contracts
                .Include(c => c.Client)
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
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name");
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract, IFormFile signedAgreement)
        {
            if (signedAgreement != null && signedAgreement.Length > 0)
            {
                // Validate PDF
                if (!_fileValidationService.IsValidPdf(signedAgreement))
                {
                    ModelState.AddModelError("SignedAgreement", "Only PDF files are allowed.");
                    ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name", contract.ClientId);
                    return View(contract);
                }

                // Save file
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "contracts");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(signedAgreement.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await signedAgreement.CopyToAsync(stream);
                }

                contract.SignedAgreementPath = $"/uploads/contracts/{uniqueFileName}";
                contract.SignedAgreementFileName = signedAgreement.FileName;
            }

            if (ModelState.IsValid)
            {
                _context.Add(contract);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // GET: Contracts/Search
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

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(c => c.Status == status);
            }

            var contracts = await query.ToListAsync();
            ViewData["StatusFilter"] = new SelectList(new[] { "All", "Draft", "Active", "Expired", "OnHold" }, status);
            
            return View("Index", contracts);
        }

        // GET: Contracts/DownloadAgreement/5
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, contract.SignedAgreementPath.TrimStart('/'));
            
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", contract.SignedAgreementFileName);
        }
    }
}