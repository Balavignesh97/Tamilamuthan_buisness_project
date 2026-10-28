namespace Dharmacorpus.Web.Models;

/// <summary>Per-page SEO data rendered by the layout (title, description, canonical, JSON-LD).</summary>
public sealed class SeoMeta
{
    public required string Title { get; init; }
    public required string Description { get; init; }

    /// <summary>Site-relative path, e.g. "/practice-areas/gst".</summary>
    public required string Path { get; init; }

    public string OgType { get; init; } = "website";
    public bool NoIndex { get; init; }

    /// <summary>Breadcrumb trail (name, path). Home is added automatically.</summary>
    public IReadOnlyList<(string Name, string Path)> Breadcrumbs { get; init; } = [];

    /// <summary>Extra JSON-LD nodes for this page (serialised into the page @graph).</summary>
    public IReadOnlyList<object> JsonLd { get; init; } = [];

    /// <summary>Which main-menu item to highlight.</summary>
    public string? NavKey { get; init; }
}

public static class SeoViewDataExtensions
{
    private const string Key = "Seo";

    public static void SetSeo(this Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary viewData, SeoMeta seo) =>
        viewData[Key] = seo;

    public static SeoMeta? GetSeo(this Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary viewData) =>
        viewData[Key] as SeoMeta;
}
