namespace TechMove.Dtos.Responses;

public class ContractResponse
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ServiceLevel { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public decimal ContractValueUSD { get; set; }
    public decimal ContractValueZAR { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int DaysUntilExpiry { get; set; }

    // Path (relative or absolute) to stored signed agreement PDF
    public string? SignedAgreementPath { get; set; }

    // Original or stored file name for the signed agreement
    public string? SignedAgreementFileName { get; set; }
}