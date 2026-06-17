using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.Dtos.Requests;
using TechMove.Dtos.Responses;
using TechMove.Models;
using TechMove.Services;

namespace TechMove.Controllers
{
    [Authorize]
    public class ContractsController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly IFileValidationService _fileValidationService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(
            IApiClient apiClient,
            IFileValidationService fileValidationService,
            IWebHostEnvironment environment,
            ILogger<ContractsController> logger)
        {
            _apiClient = apiClient;
            _fileValidationService = fileValidationService;
            _environment = environment;
            _logger = logger;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(string? status)
        {
            var contractDtos = await _apiClient.GetContractsAsync(status);
            var contracts = contractDtos.Select(dto => new Contract
            {
                Id = dto.Id,
                ClientId = dto.ClientId,
                Client = new Client { Name = dto.ClientName },
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                ServiceLevel = dto.ServiceLevel,
                ContractNumber = dto.ContractNumber,
                ContractValueUSD = dto.ContractValueUSD,
                ContractValueZAR = dto.ContractValueZAR,
                CreatedDate = dto.CreatedDate,
                LastModifiedDate = dto.LastModifiedDate
            }).ToList();
            return View(contracts);
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var dto = await _apiClient.GetContractAsync(id.Value);
            if (dto == null) return NotFound();

            var contract = new Contract
            {
                Id = dto.Id,
                ClientId = dto.ClientId,
                Client = new Client { Name = dto.ClientName },
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                ServiceLevel = dto.ServiceLevel,
                ContractNumber = dto.ContractNumber,
                ContractValueUSD = dto.ContractValueUSD,
                ContractValueZAR = dto.ContractValueZAR,
                CreatedDate = dto.CreatedDate,
                LastModifiedDate = dto.LastModifiedDate
            };
            return View(contract);
        }

        // GET: Contracts/Create
        [Authorize(Roles = "Admin,LogisticsManager")]
        public async Task<IActionResult> Create()
        {
            // Hardcoded clients for now (API call failing)
            var clients = new List<ClientResponse>
            {
                new ClientResponse { Id = 1, Name = "Acme Global" },
                new ClientResponse { Id = 2, Name = "Test Company" },
                new ClientResponse { Id = 3, Name = "TechMove Solutions" }
            };
            ViewData["ClientId"] = new SelectList(clients, "Id", "Name");
            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,LogisticsManager")]
        public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,Status,ServiceLevel,ContractNumber,ContractValueUSD")] CreateContractRequest contract, IFormFile? signedAgreement)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var created = await _apiClient.CreateContractAsync(contract);

                    if (signedAgreement != null && signedAgreement.Length > 0)
                    {
                        if (!_fileValidationService.IsValidPdf(signedAgreement))
                        {
                            ModelState.AddModelError("SignedAgreement", "Only PDF files are allowed.");
                            // Map DTO to Contract for the view
                            var viewModel = new Contract
                            {
                                ClientId = contract.ClientId,
                                StartDate = contract.StartDate,
                                EndDate = contract.EndDate,
                                Status = contract.Status ?? "Draft",
                                ServiceLevel = contract.ServiceLevel,
                                ContractNumber = contract.ContractNumber,
                                ContractValueUSD = contract.ContractValueUSD
                            };
                            var clients = new List<ClientResponse>
                            {
                                new ClientResponse { Id = 1, Name = "Acme Global" },
                                new ClientResponse { Id = 2, Name = "Test Company" },
                                new ClientResponse { Id = 3, Name = "TechMove Solutions" }
                            };
                            ViewData["ClientId"] = new SelectList(clients, "Id", "Name", contract.ClientId);
                            return View(viewModel);
                        }
                    }

                    TempData["SuccessMessage"] = "Contract created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating contract");
                    ModelState.AddModelError("", "Failed to create contract. Please try again.");
                }
            }

            // On validation error or exception, map DTO to Contract for the view
            var model = new Contract
            {
                ClientId = contract.ClientId,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status ?? "Draft",
                ServiceLevel = contract.ServiceLevel,
                ContractNumber = contract.ContractNumber,
                ContractValueUSD = contract.ContractValueUSD
            };
            var clientsList = new List<ClientResponse>
            {
                new ClientResponse { Id = 1, Name = "Acme Global" },
                new ClientResponse { Id = 2, Name = "Test Company" },
                new ClientResponse { Id = 3, Name = "TechMove Solutions" }
            };
            ViewData["ClientId"] = new SelectList(clientsList, "Id", "Name", contract.ClientId);
            return View(model);
        }


        // GET: Contracts/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var dto = await _apiClient.GetContractAsync(id.Value);
            if (dto == null) return NotFound();

            var contract = new Contract
            {
                Id = dto.Id,
                ClientId = dto.ClientId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                ServiceLevel = dto.ServiceLevel,
                ContractNumber = dto.ContractNumber,
                ContractValueUSD = dto.ContractValueUSD
            };
            // Hardcoded clients for edit too
            var clients = new List<ClientResponse>
            {
                new ClientResponse { Id = 1, Name = "Acme Global" },
                new ClientResponse { Id = 2, Name = "Test Company" },
                new ClientResponse { Id = 3, Name = "TechMove Solutions" }
            };
            ViewData["ClientId"] = new SelectList(clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (id != contract.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Note: API doesn't have PUT /contracts/{id} yet, but we have PATCH for status.
                    // For full update, either implement PUT endpoint or combine create+delete.
                    // For simplicity, we'll only allow status update via separate action.
                    TempData["WarningMessage"] = "Full contract edit not supported via API. Use Status update instead.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating contract");
                    ModelState.AddModelError("", "Failed to update contract.");
                }
            }

            var clients = new List<ClientResponse>
            {
                new ClientResponse { Id = 1, Name = "Acme Global" },
                new ClientResponse { Id = 2, Name = "Test Company" },
                new ClientResponse { Id = 3, Name = "TechMove Solutions" }
            };
            ViewData["ClientId"] = new SelectList(clients, "Id", "Name", contract.ClientId);
            return View(contract);
        }

        // POST: Contracts/UpdateStatus (convenience action)
        [HttpPost]
        [Authorize(Roles = "Admin,LogisticsManager")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var success = await _apiClient.UpdateContractStatusAsync(id, status);
            if (success)
                TempData["SuccessMessage"] = $"Contract status updated to {status}";
            else
                TempData["ErrorMessage"] = "Failed to update status";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Contracts/Search
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Search(DateTime? startDate, DateTime? endDate, string status)
        {
            // Since API supports filtering only by status, we'll get all and filter in memory for date range.
            var allContracts = await _apiClient.GetContractsAsync(status);
            var filtered = allContracts.AsEnumerable();

            if (startDate.HasValue)
                filtered = filtered.Where(c => c.StartDate >= startDate.Value);
            if (endDate.HasValue)
                filtered = filtered.Where(c => c.EndDate <= endDate.Value);

            var contracts = filtered.Select(dto => new Contract
            {
                Id = dto.Id,
                ClientId = dto.ClientId,
                Client = new Client { Name = dto.ClientName },
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                ServiceLevel = dto.ServiceLevel,
                ContractNumber = dto.ContractNumber,
                ContractValueUSD = dto.ContractValueUSD,
                ContractValueZAR = dto.ContractValueZAR,
                CreatedDate = dto.CreatedDate,
                LastModifiedDate = dto.LastModifiedDate
            }).ToList();

            ViewData["StatusFilter"] = new SelectList(new[] { "All", "Draft", "Active", "Expired", "On Hold" }, status ?? "All");
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            return View("Index", contracts);
        }

        // GET: Contracts/DownloadAgreement/5
        // Note: PDF files are stored on server; if you moved file storage to API, you'd need to stream from API.
        // For now, this action still expects files in local wwwroot. To fully decouple, create API endpoint /api/contracts/{id}/download.
        public async Task<IActionResult> DownloadAgreement(int id)
        {
            var contract = await _apiClient.GetContractAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                return NotFound();

            // Assuming files are still stored in the MVC's wwwroot. If files are on API server, you'd proxy.
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
            var success = await _apiClient.DeleteContractAsync(id);
            if (success)
                TempData["SuccessMessage"] = "Contract deleted successfully!";
            else
                TempData["ErrorMessage"] = "Failed to delete contract.";
            return RedirectToAction(nameof(Index));
        }
    }
}