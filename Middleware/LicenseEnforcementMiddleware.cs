using OnlinePizzaWebApplication.Services;

namespace OnlinePizzaWebApplication.Middleware
{
    /// <summary>
    /// Blocks access to the whole system when there is no valid license. Only the
    /// activation/expired screens and static assets remain reachable so the operator
    /// can enter a new key.
    /// </summary>
    public class LicenseEnforcementMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly string[] AllowedPrefixes =
        {
            "/License",
            "/css", "/js", "/lib", "/images", "/img", "/favicon"
        };

        public LicenseEnforcementMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILicenseService licenseService)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            if (IsAllowed(path))
            {
                await _next(context);
                return;
            }

            var status = await licenseService.GetStatusAsync();
            if (status.IsValid)
            {
                await _next(context);
                return;
            }

            var target = status.State == LicenseState.NotActivated
                ? "/License/Activate"
                : "/License/Blocked";
            context.Response.Redirect(target);
        }

        private static bool IsAllowed(string path)
        {
            foreach (var prefix in AllowedPrefixes)
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
