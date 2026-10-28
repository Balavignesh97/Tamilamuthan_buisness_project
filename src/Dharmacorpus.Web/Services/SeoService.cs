using System.Text.Json;
using System.Text.Json.Serialization;
using Dharmacorpus.Web.Infrastructure;
using Dharmacorpus.Web.Models;
using Microsoft.Extensions.Options;

namespace Dharmacorpus.Web.Services;

/// <summary>
/// Builds absolute URLs and schema.org JSON-LD so Google and AI crawlers can read exactly who the
/// practice is, where it is, what it does and who its advocates are.
/// </summary>
public sealed class SeoService(IOptions<SiteOptions> options, ContentService content)
{
    // Default encoder escapes <, > and & so content can never break out of the <script> block.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public string BaseUrl { get; } = options.Value.BaseUrl.TrimEnd('/');

    public string Absolute(string path) => path == "/" ? BaseUrl + "/" : BaseUrl + path;

    private string OrgId => BaseUrl + "/#organization";
    private string SiteId => BaseUrl + "/#website";

    public string BuildJsonLd(SeoMeta seo)
    {
        var graph = new List<object> { Organization(), WebSite() };

        var crumbs = new List<(string Name, string Path)> { ("Home", "/") };
        crumbs.AddRange(seo.Breadcrumbs);
        if (crumbs.Count > 1)
        {
            graph.Add(new Dictionary<string, object?>
            {
                ["@type"] = "BreadcrumbList",
                ["itemListElement"] = crumbs.Select((c, i) => new Dictionary<string, object?>
                {
                    ["@type"] = "ListItem",
                    ["position"] = i + 1,
                    ["name"] = c.Name,
                    ["item"] = Absolute(c.Path)
                }).ToList()
            });
        }

        graph.AddRange(seo.JsonLd);

        var doc = new Dictionary<string, object?> { ["@context"] = "https://schema.org", ["@graph"] = graph };
        var node = JsonSerializer.SerializeToNode(doc, JsonOptions)!;
        RemoveNulls(node);
        return node.ToJsonString(JsonOptions);
    }

    /// <summary>Drops properties whose value is null (e.g. an address field not yet provided).</summary>
    private static void RemoveNulls(System.Text.Json.Nodes.JsonNode? node)
    {
        switch (node)
        {
            case System.Text.Json.Nodes.JsonObject obj:
                foreach (var key in obj.Where(kv => kv.Value is null).Select(kv => kv.Key).ToList())
                    obj.Remove(key);
                foreach (var kv in obj) RemoveNulls(kv.Value);
                break;
            case System.Text.Json.Nodes.JsonArray arr:
                foreach (var item in arr) RemoveNulls(item);
                break;
        }
    }

    public object Organization()
    {
        var s = content.Site;
        return new Dictionary<string, object?>
        {
            ["@type"] = "LegalService",
            ["@id"] = OrgId,
            ["name"] = s.Name,
            ["url"] = BaseUrl + "/",
            ["description"] = s.Description,
            ["slogan"] = s.Tagline,
            ["telephone"] = s.Phone,
            ["email"] = s.Email,
            ["address"] = new Dictionary<string, object?>
            {
                ["@type"] = "PostalAddress",
                ["streetAddress"] = string.IsNullOrWhiteSpace(s.StreetAddress) ? null : s.StreetAddress,
                ["postalCode"] = string.IsNullOrWhiteSpace(s.PostalCode) ? null : s.PostalCode,
                ["addressLocality"] = s.Locality,
                ["addressRegion"] = s.Region,
                ["addressCountry"] = s.Country
            },
            ["openingHours"] = string.IsNullOrWhiteSpace(s.OpeningHours) ? null : s.OpeningHours,
            ["areaServed"] = new Dictionary<string, object?> { ["@type"] = "State", ["name"] = s.Region },
            ["knowsAbout"] = content.PracticeAreas.Select(p => p.Title).ToList(),
            ["employee"] = content.Advocates.Select(a => new Dictionary<string, object?> { ["@id"] = PersonId(a) }).ToList()
        };
    }

    private object WebSite() => new Dictionary<string, object?>
    {
        ["@type"] = "WebSite",
        ["@id"] = SiteId,
        ["url"] = BaseUrl + "/",
        ["name"] = content.Site.Name,
        ["inLanguage"] = "en-IN",
        ["publisher"] = new Dictionary<string, object?> { ["@id"] = OrgId }
    };

    public object PracticeService(PracticeArea area) => new Dictionary<string, object?>
    {
        ["@type"] = "Service",
        ["@id"] = Absolute($"/practice-areas/{area.Slug}") + "#service",
        ["name"] = area.Title,
        ["description"] = area.Summary,
        ["serviceType"] = area.Title,
        ["provider"] = new Dictionary<string, object?> { ["@id"] = OrgId },
        ["areaServed"] = new Dictionary<string, object?> { ["@type"] = "State", ["name"] = content.Site.Region },
        ["url"] = Absolute($"/practice-areas/{area.Slug}")
    };

    public string PersonId(Advocate a) => Absolute($"/team/{a.Slug}") + "#person";

    public object Person(Advocate a) => new Dictionary<string, object?>
    {
        ["@type"] = "Person",
        ["@id"] = PersonId(a),
        ["name"] = a.Name,
        ["jobTitle"] = a.Titles,
        ["worksFor"] = new Dictionary<string, object?> { ["@id"] = OrgId },
        ["url"] = Absolute($"/team/{a.Slug}"),
        ["sameAs"] = string.IsNullOrWhiteSpace(a.LinkedIn) ? null : new[] { a.LinkedIn }
    };

    public object FaqPage(IEnumerable<Faq> faqs, string path) => new Dictionary<string, object?>
    {
        ["@type"] = "FAQPage",
        ["@id"] = Absolute(path) + "#faq",
        ["mainEntity"] = faqs.Select(f => new Dictionary<string, object?>
        {
            ["@type"] = "Question",
            ["name"] = f.Question,
            ["acceptedAnswer"] = new Dictionary<string, object?> { ["@type"] = "Answer", ["text"] = f.Answer }
        }).ToList()
    };
}
