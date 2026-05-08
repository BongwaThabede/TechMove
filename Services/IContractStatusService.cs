using TechMove.Models;

namespace TechMove.Services
{
    public interface IContractStatusService
    {
        Task SyncAllAsync(DateTime utcToday);
        bool SyncSingle(Contract contract, DateTime utcToday);
    }
}
