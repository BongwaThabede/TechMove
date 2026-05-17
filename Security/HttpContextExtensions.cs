using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace TechMove.Security
{
    public static class HttpContextExtensions
    {
        public static bool IsLoggedIn(this HttpContext context)
        {
            return context.User?.Identity?.IsAuthenticated == true;
        }

        public static bool HasRole(this HttpContext context, string role)
        {
            return context.User?.IsInRole(role) == true;
        }

        public static bool HasAnyRole(this HttpContext context, params string[] roles)
        {
            if (roles == null || roles.Length == 0) return false;
            return roles.Any(r => context.User?.IsInRole(r) == true);
        }

        public static string? GetUserId(this HttpContext context)
        {
            return context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string? GetUserEmail(this HttpContext context)
        {
            return context.User?.Identity?.Name;
        }

        public static string? GetCurrentUser(this HttpContext context)
        {
            return context.User?.Identity?.Name;
        }

        public static string? GetCurrentRole(this HttpContext context)
        {
            return context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        }
    }
}