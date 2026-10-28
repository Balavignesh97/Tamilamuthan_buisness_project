using Microsoft.Extensions.Options;

namespace Dharmacorpus.Web.Infrastructure;

/// <summary>
/// Sends visitors on the bare domain (dharmacorpus.in) to the canonical host
/// (www.dharmacorpus.in) with a permanent redirect, so search engines index one version.
/// </summary>
public sealed class CanonicalHostMiddleware(RequestDelegate next, IOptions<SiteOptions> options)
{
    private readonly string? _canonicalHost = options.Value.CanonicalHost;

    public Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;
        if (!string.IsNullOrWhiteSpace(_canonicalHost)
            && !string.Equals(host, _canonicalHost, StringComparison.OrdinalIgnoreCase)
            && _canonicalHost.EndsWith(host, StringComparison.OrdinalIgnoreCase)) // only the bare-domain variant
        {
            var target = $"https://{_canonicalHost}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
            context.Response.Redirect(target, permanent: true);
            return Task.CompletedTask;
        }
        return next(context);
    }
}
