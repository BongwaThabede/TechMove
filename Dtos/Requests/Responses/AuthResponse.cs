namespace TechMove.Dtos.Responses;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public int ExpiresIn { get; set; } // seconds
    public DateTime ExpiresAt => DateTime.UtcNow.AddSeconds(ExpiresIn);
}