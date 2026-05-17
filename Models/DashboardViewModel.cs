namespace TechMove.Models
{
    public class DashboardViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int ActiveContracts { get; set; }
        public int PendingRequests { get; set; }
        public int TotalClients { get; set; }
        public decimal CurrencyRateUsdToZar { get; set; }
        public List<RecentContractItem> RecentActivity { get; set; } = new();

        public class RecentContractItem
        {
            public int ContractId { get; set; }
            public string ContractNumber { get; set; } = string.Empty;
            public string ClientName { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }
    }
}