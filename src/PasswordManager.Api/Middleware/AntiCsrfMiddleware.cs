namespace PasswordManager.Api.Middleware;

/// <summary>
/// Enforces custom request header verification for state-changing requests authenticated via cookies.
/// In accordance with OWASP CSRF Prevention guidelines, browsers prevent cross-origin scripts from
/// setting custom headers without explicit CORS approval, rendering forged cross-site requests impossible.
/// </summary>
public class AntiCsrfMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> StateChangingMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Delete,
        HttpMethods.Patch
    };

    public AntiCsrfMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (StateChangingMethods.Contains(context.Request.Method))
        {
            // If request carries the session cookie, enforce presence of custom anti-CSRF header
            if (context.Request.Cookies.ContainsKey("pm_session"))
            {
                var hasCustomHeader =
                    context.Request.Headers.ContainsKey("X-Requested-With") ||
                    context.Request.Headers.ContainsKey("X-CSRF-Protection");

                if (!hasCustomHeader)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/problem+json";
                    await context.Response.WriteAsync(
                        "{\"status\":403,\"title\":\"Forbidden\",\"detail\":\"CSRF validation failed: Missing custom anti-CSRF header (X-Requested-With or X-CSRF-Protection).\"}");
                    return;
                }
            }
        }

        await _next(context);
    }
}
