using TechMove.Models;

namespace TechMove.Models
{
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
}