using System.Text;
using System.Xml.Linq;
using Dharmacorpus.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Dharmacorpus.Web.Controllers;

/// <summary>robots.txt, sitemap.xml and llms.txt, generated from the same content as the pages.</summary>
public sealed class SeoController(ContentService content, SeoService seo) : Controller
{
    private IEnumerable<string> AllPaths()
    {
        yield return "/";
        yield return "/about";
        yield return "/practice-areas";
        foreach (var p in content.PracticeAreas) yield return $"/practice-areas/{p.Slug}";
        yield return "/team";
        foreach (var a in content.Advocates) yield return $"/team/{a.Slug}";
        yield return "/contact";
        yield return "/disclaimer";
        yield return "/privacy-policy";
        yield return "/terms-of-use";
    }

    [HttpGet("robots.txt")]
    [OutputCache(PolicyName = "content")]
    public ContentResult Robots()
    {
        // Search and AI crawlers are welcome; everything here is public information.
        var sb = new StringBuilder()
            .AppendLine("User-agent: *")
            .AppendLine("Allow: /")
            .AppendLine("Disallow: /contact/thank-you")
            .AppendLine()
            .AppendLine("# AI search and answer engines")
            .AppendLine("User-agent: GPTBot").AppendLine("User-agent: OAI-SearchBot").AppendLine("User-agent: ChatGPT-User")
            .AppendLine("User-agent: ClaudeBot").AppendLine("User-agent: Claude-SearchBot").AppendLine("User-agent: Claude-User")
            .AppendLine("User-agent: PerplexityBot").AppendLine("User-agent: Google-Extended").AppendLine("User-agent: Bingbot")
            .AppendLine("Allow: /")
            .AppendLine()
            .AppendLine($"Sitemap: {seo.Absolute("/sitemap.xml")}");
        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    [HttpGet("sitemap.xml")]
    [OutputCache(PolicyName = "content")]
    public ContentResult Sitemap()
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        var urlset = new XElement(ns + "urlset",
            AllPaths().Select(p => new XElement(ns + "url",
                new XElement(ns + "loc", seo.Absolute(p)),
                new XElement(ns + "changefreq", p == "/" ? "weekly" : "monthly"),
                new XElement(ns + "priority", p == "/" ? "1.0" : p.Count(c => c == '/') == 1 ? "0.8" : "0.6"))));
        var xml = new XDocument(new XDeclaration("1.0", "utf-8", null), urlset);
        return Content(xml.Declaration + Environment.NewLine + xml.Root, "application/xml", Encoding.UTF8);
    }

    [HttpGet("llms.txt")]
    [OutputCache(PolicyName = "content")]
    public ContentResult Llms()
    {
        var s = content.Site;
        var sb = new StringBuilder()
            .AppendLine($"# {s.Name}")
            .AppendLine()
            .AppendLine($"> {s.Description}")
            .AppendLine()
            .AppendLine($"{s.Name} is a practice of {s.AdvocateCount} advocates based in {s.Locality}, {s.Region}, {s.CountryName}, with {s.YearsCombined} years of combined legal practice. Its advocates appear before District Courts, High Courts and tribunals and statutory forums. Contact: {s.PhoneDisplay}, {s.Email}. Information on the site is general and is not legal advice.")
            .AppendLine()
            .AppendLine("## Practice areas")
            .AppendLine();
        foreach (var p in content.PracticeAreas)
            sb.AppendLine($"- [{p.Title}]({seo.Absolute($"/practice-areas/{p.Slug}")}): {p.Summary}");
        sb.AppendLine().AppendLine("## Advocates").AppendLine();
        foreach (var a in content.Advocates)
            sb.AppendLine($"- [{a.Name}]({seo.Absolute($"/team/{a.Slug}")}): {a.Titles}");
        sb.AppendLine().AppendLine("## Other pages").AppendLine()
            .AppendLine($"- [About]({seo.Absolute("/about")})")
            .AppendLine($"- [Contact]({seo.Absolute("/contact")})")
            .AppendLine($"- [Disclaimer]({seo.Absolute("/disclaimer")})");
        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }
}
