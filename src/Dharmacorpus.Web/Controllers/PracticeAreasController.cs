using Dharmacorpus.Web.Models;
using Dharmacorpus.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Dharmacorpus.Web.Controllers;

[Route("practice-areas")]
public sealed class PracticeAreasController(ContentService content, SeoService seo) : Controller
{
    [HttpGet("")]
    [OutputCache(PolicyName = "content")]
    public IActionResult Index()
    {
        ViewData.SetSeo(new SeoMeta
        {
            Title = "Practice Areas | Dharmacorpus, Dharmapuri",
            Description = "Dharmacorpus practises across litigation, arbitration, corporate and commercial law, property and revenue, criminal law, writ jurisdiction, PMLA, GST, technology, intellectual property and regulatory matters.",
            Path = "/practice-areas",
            NavKey = "practice",
            Breadcrumbs = [("Practice Areas", "/practice-areas")],
            JsonLd = content.PracticeAreas.Select(seo.PracticeService).ToList()
        });
        return View(content);
    }

    [HttpGet("{slug}")]
    [OutputCache(PolicyName = "content")]
    public IActionResult Detail(string slug)
    {
        var area = content.FindPracticeArea(slug);
        if (area is null) return NotFound();

        ViewData.SetSeo(new SeoMeta
        {
            Title = $"{area.Title} Lawyers in Dharmapuri | Dharmacorpus",
            Description = area.Summary,
            Path = $"/practice-areas/{area.Slug}",
            NavKey = "practice",
            Breadcrumbs = [("Practice Areas", "/practice-areas"), (area.Title, $"/practice-areas/{area.Slug}")],
            JsonLd = [seo.PracticeService(area)]
        });

        var related = area.Related.Select(content.FindPracticeArea).OfType<PracticeArea>().ToList();
        return View(new PracticeDetailViewModel(area, related, content.Site));
    }
}

public sealed record PracticeDetailViewModel(PracticeArea Area, IReadOnlyList<PracticeArea> Related, SiteInfo Site);
