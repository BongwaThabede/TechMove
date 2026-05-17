namespace TechMove.Dtos.Responses;

public class ClientResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactDetails { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int ActiveContractsCount { get; set; }
}