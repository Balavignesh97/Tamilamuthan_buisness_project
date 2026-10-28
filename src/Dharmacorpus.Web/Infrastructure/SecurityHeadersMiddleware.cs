namespace Dharmacorpus.Web.Infrastructure;

/// <summary>
/// Adds browser security headers to every response. All scripts, styles and fonts are
/// self-hosted, so the Content-Security-Policy can stay strict ('self' only).
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const string ContentSecurityPolicy =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "form-action 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "object-src 'none'";

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var h = context.Response.Headers;
            h["Content-Security-Policy"] = ContentSecurityPolicy;
            h["X-Content-Type-Options"] = "nosniff";
            h["X-Frame-Options"] = "DENY";
            h["Referrer-Policy"] = "strict-origin-when-cross-origin";
            h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), interest-cohort=()";
            h["Cross-Origin-Opener-Policy"] = "same-origin";
            h.Remove("X-Powered-By");
            h.Remove("Server");
            return Task.CompletedTask;
        });
        return next(context);
    }
}
