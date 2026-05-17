using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;

namespace TechMove.Services
{
    public class ContractStatusService : IContractStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractStatusService> _logger;

        public ContractStatusService(ApplicationDbContext context, ILogger<ContractStatusService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SyncAllAsync(DateTime utcToday)
        {
            var contracts = await _context.Contracts.ToListAsync();
            bool anyChanges = false;
            
            foreach (var contract in contracts)
            {
                if (SyncSingle(contract, utcToday))
                    anyChanges = true;
            }
            
            if (anyChanges)
                await _context.SaveChangesAsync();
        }

        public bool SyncSingle(Contract contract, DateTime utcToday)
        {
            bool changed = false;
            
            // Update status based on dates
            if (contract.Status == "Active" && contract.EndDate.Date < utcToday.Date)
            {
                contract.Status = "Expired";
                changed = true;
                _logger.LogInformation("Contract {ContractNumber} expired on {Date}", contract.ContractNumber, utcToday);
            }
            else if (contract.Status == "Draft" && contract.StartDate.Date <= utcToday.Date && contract.EndDate.Date >= utcToday.Date)
            {
                contract.Status = "Active";
                changed = true;
                _logger.LogInformation("Contract {ContractNumber} activated on {Date}", contract.ContractNumber, utcToday);
            }
            
            return changed;
        }
    }
}