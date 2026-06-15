namespace TechMove.Services
{
    public class ContractResponse
    {
        public int ClientId { get; internal set; }
        public string ClientName { get; internal set; }
        public DateTime StartDate { get; internal set; }
        public DateTime EndDate { get; internal set; }
        public int Id { get; internal set; }
        public string Status { get; internal set; }
        public string ServiceLevel { get; internal set; }
        public string ContractNumber { get; internal set; }
        public decimal ContractValueUSD { get; internal set; }
        public decimal ContractValueZAR { get; internal set; }
        public DateTime CreatedDate { get; internal set; }
        public DateTime? LastModifiedDate { get; internal set; }
        public string? SignedAgreementPath { get; internal set; }
        public string? SignedAgreementFileName { get; internal set; }
        public int DaysUntilExpiry { get; internal set; }
    }
}