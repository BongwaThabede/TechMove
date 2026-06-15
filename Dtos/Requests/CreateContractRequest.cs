namespace TechMove.Dtos.Requests;

public class CreateContractRequest
{
    public int ClientId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Status { get; set; }
    public string ServiceLevel { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public decimal ContractValueUSD { get; set; }
}

public class UpdateContractStatusRequest
{
    public string Status { get; set; } = string.Empty;
}