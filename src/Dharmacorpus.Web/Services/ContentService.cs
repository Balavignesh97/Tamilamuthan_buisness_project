using System.Text.Json;
using Dharmacorpus.Web.Models;

namespace Dharmacorpus.Web.Services;

/// <summary>
/// Loads the site's content (practice areas, advocates, FAQs) from JSON files in /Content once
/// at startup and keeps it in memory. Editing a JSON file and restarting the app updates the site.
/// </summary>
public sealed class ContentService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public SiteInfo Site { get; }
    public IReadOnlyList<PracticeArea> PracticeAreas { get; }
    public IReadOnlyList<Advocate> Advocates { get; }
    public IReadOnlyList<Faq> Faqs { get; }

    /// <summary>Practice groups in display order.</summary>
    public IReadOnlyList<string> Groups { get; }

    public ContentService(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.ContentRootPath, "Content", "en");

        Site = Load<SiteInfo>(dir, "site.json");
        var areas = Load<List<PracticeArea>>(dir, "practice-areas.json");
        for (var i = 0; i < areas.Count; i++) areas[i].Number = i + 1;
        PracticeAreas = areas;
        Advocates = Load<List<Advocate>>(dir, "team.json");
        Faqs = Load<List<Faq>>(dir, "faqs.json");
        Groups = areas.Select(a => a.Group).Distinct().ToList();

        // Fail fast on broken cross-links between content files.
        var slugs = areas.Select(a => a.Slug).ToHashSet();
        foreach (var a in areas)
            foreach (var r in a.Related.Where(r => !slugs.Contains(r)))
                throw new InvalidOperationException($"Practice area '{a.Slug}' links to unknown area '{r}'.");
    }

    public PracticeArea? FindPracticeArea(string slug) =>
        PracticeAreas.FirstOrDefault(p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public Advocate? FindAdvocate(string slug) =>
        Advocates.FirstOrDefault(a => string.Equals(a.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public static string GroupId(string group) =>
        new string(group.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray())
            .Replace("---", "-").Trim('-');

    private static T Load<T>(string dir, string file)
    {
        var path = Path.Combine(dir, file);
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<T>(stream, JsonOptions)
               ?? throw new InvalidOperationException($"Content file '{file}' is empty.");
    }
}
