using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TechMove.Data;
using TechMove.Models;

[Authorize(Policy = "ClientOnly")]
public class ClientPortalController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ClientPortalController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        
        var clientId = await GetUserClientIdAsync(user.Id);
        if (clientId == null) return RedirectToAction("LinkAccount");
        
        var client = await _context.Clients.FindAsync(clientId.Value);
        if (client == null) return NotFound();
        
        var model = new ClientDashboardViewModel
        {
            Client = client,
            ActiveContracts = await _context.Contracts
                .Where(c => c.ClientId == clientId.Value && c.Status == "Active")
                .ToListAsync(),
            OpenRequests = await _context.ServiceRequests
                .Include(sr => sr.Contract)
                .Where(sr => sr.Status == "Open" && sr.Contract != null && sr.Contract.ClientId == clientId.Value)
                .ToListAsync()
        };
        
        return View(model);
    }

    public async Task<IActionResult> MyContracts()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        
        var clientId = await GetUserClientIdAsync(user.Id);
        if (clientId == null) return NotFound();
        
        var contracts = await _context.Contracts
            .Where(c => c.ClientId == clientId.Value)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
            
        return View(contracts);
    }

    public async Task<IActionResult> MyServiceRequests()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        
        var clientId = await GetUserClientIdAsync(user.Id);
        if (clientId == null) return NotFound();
        
        var requests = await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .Where(sr => sr.Contract != null && sr.Contract.ClientId == clientId.Value)
            .OrderByDescending(sr => sr.CreatedDate)
            .ToListAsync();
            
        return View(requests);
    }

    public async Task<IActionResult> CreateServiceRequest(int? contractId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        
        var clientId = await GetUserClientIdAsync(user.Id);
        if (clientId == null) return NotFound();
        
        ViewBag.Contracts = await _context.Contracts
            .Where(c => c.ClientId == clientId.Value && c.Status == "Active")
            .ToListAsync();
            
        if (contractId.HasValue)
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == contractId.Value && c.ClientId == clientId.Value);
            if (contract == null) return NotFound();
        }
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateServiceRequest(ServiceRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");
        
        var clientId = await GetUserClientIdAsync(user.Id);
        if (clientId == null) return NotFound();
        
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == request.ContractId && 
                                     c.ClientId == clientId.Value && 
                                     c.Status == "Active");
        
        if (contract == null)
        {
            ModelState.AddModelError("ContractId", "Invalid or inactive contract.");
            ViewBag.Contracts = await _context.Contracts
                .Where(c => c.ClientId == clientId.Value && c.Status == "Active")
                .ToListAsync();
            return View(request);
        }
        
        request.CreatedDate = DateTime.UtcNow;
        request.Status = "Open";
        request.Priority = "Medium";
        request.RequestDate = DateTime.UtcNow;
        request.RequestNumber = $"SR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        request.Cost = 0;
        request.CostInZAR = 0;
        
        _context.ServiceRequests.Add(request);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(MyServiceRequests));
    }
    
    private async Task<int?> GetUserClientIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;
        
        var claims = await _userManager.GetClaimsAsync(user);
        var clientIdClaim = claims.FirstOrDefault(c => c.Type == "ClientId");
        
        if (clientIdClaim != null && int.TryParse(clientIdClaim.Value, out int clientId))
        {
            return clientId;
        }
        
        return null;
    }
}

public class ClientDashboardViewModel
{
    public Client? Client { get; set; }
    public List<Contract> ActiveContracts { get; set; } = new();
    public List<ServiceRequest> OpenRequests { get; set; } = new();
}