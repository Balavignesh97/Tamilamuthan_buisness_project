namespace Dharmacorpus.Web.Infrastructure;

/// <summary>Site-wide settings from the "Site" section of appsettings.json.</summary>
public sealed class SiteOptions
{
    public const string SectionName = "Site";

    /// <summary>Absolute base URL used for canonical links, sitemap and structured data.</summary>
    public string BaseUrl { get; set; } = "https://www.dharmacorpus.in";

    /// <summary>
    /// When set (e.g. "www.dharmacorpus.in"), requests for other public hosts such as the bare
    /// domain are 301-redirected here. Local and preview hosts are never redirected.
    /// </summary>
    public string? CanonicalHost { get; set; }
}

/// <summary>
/// Mail settings for delivering enquiries. Keep the password out of source control:
/// set it with an environment variable (Smtp__Password) or dotnet user-secrets.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? From { get; set; }
    public string? To { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(From) && !string.IsNullOrWhiteSpace(To);
}
