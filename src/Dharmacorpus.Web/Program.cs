using System.Threading.RateLimiting;
using Dharmacorpus.Web.Infrastructure;
using Dharmacorpus.Web.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));

builder.Services.AddSingleton<ContentService>();
builder.Services.AddSingleton<SeoService>();
builder.Services.AddSingleton<EnquiryService>();

builder.Services.AddRouting(o =>
{
    o.LowercaseUrls = true;
    o.AppendTrailingSlash = false;
});
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "X-CSRF-TOKEN";
    o.Cookie.Name = "dc.af";
    o.Cookie.SameSite = SameSiteMode.Strict;
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<BrotliCompressionProvider>();
    o.Providers.Add<GzipCompressionProvider>();
});
// Opt-in only (see [OutputCache(PolicyName = "content")]); pages with forms are never cached
// because they carry a per-visitor anti-forgery token.
builder.Services.AddOutputCache(o => o.AddPolicy("content", p => p.Expire(TimeSpan.FromMinutes(10))));

// Limit enquiry submissions per client IP to stop form spam.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("enquiry", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 }));
});

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddHsts(o =>
{
    o.MaxAge = TimeSpan.FromDays(365);
    o.IncludeSubDomains = true;
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseMiddleware<CanonicalHostMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Fonts and vendor libraries are versioned by the build, so cache them long.
        var path = ctx.Context.Request.Path.Value ?? string.Empty;
        var maxAge = path.StartsWith("/fonts/") || path.StartsWith("/lib/") ? 31536000 : 86400;
        ctx.Context.Response.Headers.CacheControl = $"public,max-age={maxAge}";
    }
});

app.UseRouting();
app.UseRateLimiter();
app.UseOutputCache();
app.UseAntiforgery();

app.MapControllers();

app.Run();
