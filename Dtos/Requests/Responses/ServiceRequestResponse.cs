namespace TechMove.Dtos.Responses;

public class ServiceRequestResponse
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int ContractId { get; set; }
    public string ContractClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public decimal CostInZAR { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? AdminNotes { get; set; }
}