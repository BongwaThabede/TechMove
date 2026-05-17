using System.ComponentModel.DataAnnotations;

namespace TechMove.Dtos.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
    
    [Required]
    public string AccessToken { get; set; } = string.Empty;
}