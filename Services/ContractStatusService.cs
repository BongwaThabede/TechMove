using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;

namespace TechMove.Services
{
    public class ContractStatusService : IContractStatusService
    {
        private readonly ApplicationDbContext _context;

        public ContractStatusService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SyncAllAsync(DateTime utcToday)
        {
            var contracts = await _context.Contracts.ToListAsync();
            var changed = false;

            foreach (var contract in contracts)
            {
                changed = SyncSingle(contract, utcToday) || changed;
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }
        }

        public bool SyncSingle(Contract contract, DateTime utcToday)
        {
            var current = contract.Status?.Trim() ?? "Draft";
            var normalized = ResolveExpectedStatus(contract, utcToday);

            if (current.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            contract.Status = normalized;
            return true;
        }

        private static string ResolveExpectedStatus(Contract contract, DateTime utcToday)
        {
            // OnHold is kept as an explicit manual override.
            if (contract.Status.Equals("OnHold", StringComparison.OrdinalIgnoreCase))
            {
                return "OnHold";
            }

            if (utcToday.Date > contract.EndDate.Date)
            {
                return "Expired";
            }

            if (utcToday.Date >= contract.StartDate.Date)
            {
                return "Active";
            }

            return "Draft";
        }
    }
}
