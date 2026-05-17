using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TechMove.Dtos.Requests;
using TechMove.Dtos.Responses;

namespace TechMove.Controllers.Api.v1;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration config)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Invalid email or password" });

        if (await _userManager.IsLockedOutAsync(user))
            return Unauthorized(new { message = "Account is temporarily locked" });

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);
        await _userManager.ResetAccessFailedCountAsync(user);

        return Ok(new AuthResponse { Token = token, Email = user.Email!, Roles = roles.ToList(), ExpiresIn = 3600 });
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register(LoginRequest request, [FromQuery] string role = "Client")
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var allowedRoles = new[] { "Admin", "LogisticsCoordinator", "FinanceOfficer", "ContractManager", "Client" };
        if (!allowedRoles.Contains(role)) return BadRequest(new { message = "Invalid role specified" });

        if (await _userManager.FindByEmailAsync(request.Email) != null)
            return BadRequest(new { message = "User already exists" });

        var user = new IdentityUser { UserName = request.Email, Email = request.Email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, role);
        return Ok(new { message = "User registered successfully", userId = user.Id });
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // TODO: Implement refresh token logic
        return StatusCode(StatusCodes.Status501NotImplemented, new { message = "Not implemented yet" }); // ✅ Fixed: NotImplemented → StatusCode
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfoResponse>> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var clientId = claims.FirstOrDefault(c => c.Type == "ClientId")?.Value;

        return Ok(new UserInfoResponse
        {
            Id = user.Id, Email = user.Email!, Roles = roles.ToList(),
            ClientId = clientId != null ? int.Parse(clientId) : null,
            EmailConfirmed = user.EmailConfirmed, LockoutEnabled = user.LockoutEnabled
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout() => Ok(new { message = "Logged out successfully" });

    private async Task<string> GenerateJwtToken(IdentityUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

        if (roles.Contains("Client"))
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var clientIdClaim = userClaims.FirstOrDefault(c => c.Type == "ClientId");
            if (clientIdClaim != null) claims.Add(clientIdClaim);
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var mins) ? mins : 60;

        var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddMinutes(expiryMinutes), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}