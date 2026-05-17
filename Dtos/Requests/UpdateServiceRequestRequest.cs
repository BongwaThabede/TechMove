using System.ComponentModel.DataAnnotations;

namespace TechMove.Dtos.Requests;

public class UpdateServiceRequestRequest
{
    [StringLength(500)]
    public string? Description { get; set; }

    [RegularExpression("^(Pending|InProgress|Completed|Cancelled)$")]
    public string? Status { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Cost { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CostInZAR { get; set; }

    [StringLength(500)]
    public string? AdminNotes { get; set; }

    public DateTime? CompletedDate { get; set; }
}