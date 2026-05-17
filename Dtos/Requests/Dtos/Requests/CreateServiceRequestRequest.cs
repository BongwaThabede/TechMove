using System.ComponentModel.DataAnnotations;

namespace TechMove.Dtos.Requests;

public class CreateServiceRequestRequest
{
    [Required(ErrorMessage = "Contract ID is required")]
    public int ContractId { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Priority is required")]
    [RegularExpression("^(Low|Normal|High|Urgent)$", ErrorMessage = "Priority must be Low, Normal, High, or Urgent")]
    public string Priority { get; set; } = "Normal";
}