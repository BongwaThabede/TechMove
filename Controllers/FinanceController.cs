using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using Microsoft.AspNetCore.Identity;
using TechMove.Services;

[Authorize(Policy = "FinanceOfficer")]
public class FinanceController : Controller
{
    private readonly ApplicationDbContext _context;

    public FinanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var stats = new FinanceDashboardViewModel
        {
            // ✅ FIXED: SumAsync on non-nullable decimal returns decimal (0 if empty)
            TotalContractValueUSD = await _context.Contracts
                .Where(c => c.Status == "Active")
                .SumAsync(c => c.ContractValueUSD),
                
            TotalContractValueZAR = await _context.Contracts
                .Where(c => c.Status == "Active")
                .SumAsync(c => c.ContractValueZAR),
                
            PendingInvoices = await _context.Contracts
                .Where(c => c.Status == "Active" && c.SignedAgreementPath == null)
                .CountAsync(),
                
            ExpiringContracts = await _context.Contracts
                .Where(c => c.Status == "Active" && 
                           c.EndDate <= DateTime.UtcNow.AddDays(30))
                .ToListAsync()
        };
        
        return View(stats);
    }

    public async Task<IActionResult> Reports(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Contracts.AsQueryable();
        
        if (startDate.HasValue)
            query = query.Where(c => c.CreatedDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(c => c.CreatedDate <= endDate.Value.AddDays(1).AddTicks(-1));
        
        var report = await query
            .GroupBy(c => c.ServiceLevel)
            .Select(g => new FinancialReportItem
            {
                ServiceLevel = g.Key,
                ContractCount = g.Count(),
                // ✅ FIXED: SumAsync on non-nullable decimal returns decimal
                TotalValueUSD = g.Sum(c => c.ContractValueUSD),
                TotalValueZAR = g.Sum(c => c.ContractValueZAR)
            })
            .ToListAsync();
        
        return View(report);
    }
}

// ViewModel classes
public class FinanceDashboardViewModel
{
    public decimal TotalContractValueUSD { get; set; }
    public decimal TotalContractValueZAR { get; set; }
    public int PendingInvoices { get; set; }
    public List<Contract> ExpiringContracts { get; set; } = new();
}

public class FinancialReportItem
{
    public string ServiceLevel { get; set; } = string.Empty;
    public int ContractCount { get; set; }
    public decimal TotalValueUSD { get; set; }
    public decimal TotalValueZAR { get; set; }
}