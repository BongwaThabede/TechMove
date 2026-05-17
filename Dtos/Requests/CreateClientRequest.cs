using System.ComponentModel.DataAnnotations;

namespace TechMove.Dtos.Requests;

public class CreateClientRequest
{
    [Required(ErrorMessage = "Client name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact details are required")]
    [StringLength(200)]
    [EmailAddress]
    public string ContactDetails { get; set; } = string.Empty;

    [Required(ErrorMessage = "Region is required")]
    [StringLength(100)]
    public string Region { get; set; } = string.Empty;
}