namespace TechMove.Security
{
    public static class SessionAuthExtensions
    {
        private const string RoleKey = "CurrentRole";
        private const string UserKey = "CurrentUser";

        public static bool IsLoggedIn(this HttpContext httpContext)
        {
            return !string.IsNullOrWhiteSpace(httpContext.Session.GetString(RoleKey));
        }

        public static string? GetCurrentRole(this HttpContext httpContext)
        {
            return httpContext.Session.GetString(RoleKey);
        }

        public static string? GetCurrentUser(this HttpContext httpContext)
        {
            return httpContext.Session.GetString(UserKey);
        }

        public static bool HasAnyRole(this HttpContext httpContext, params string[] roles)
        {
            var currentRole = httpContext.GetCurrentRole();
            return !string.IsNullOrWhiteSpace(currentRole) &&
                   roles.Any(r => r.Equals(currentRole, StringComparison.OrdinalIgnoreCase));
        }

        public static void SignIn(this HttpContext httpContext, string username, string role)
        {
            httpContext.Session.SetString(UserKey, username);
            httpContext.Session.SetString(RoleKey, role);
        }

        public static void SignOutSession(this HttpContext httpContext)
        {
            httpContext.Session.Remove(UserKey);
            httpContext.Session.Remove(RoleKey);
        }
    }
}
