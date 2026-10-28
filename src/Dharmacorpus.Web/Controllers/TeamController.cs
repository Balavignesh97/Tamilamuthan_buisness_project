using Dharmacorpus.Web.Models;
using Dharmacorpus.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Dharmacorpus.Web.Controllers;

[Route("team")]
public sealed class TeamController(ContentService content, SeoService seo) : Controller
{
    [HttpGet("")]
    [OutputCache(PolicyName = "content")]
    public IActionResult Index()
    {
        ViewData.SetSeo(new SeoMeta
        {
            Title = "Our Advocates | Dharmacorpus, Dharmapuri",
            Description = "Meet the advocates of Dharmacorpus, a legal practice based in Dharmapuri, Tamil Nadu.",
            Path = "/team",
            NavKey = "team",
            Breadcrumbs = [("Team", "/team")],
            JsonLd = content.Advocates.Select(seo.Person).ToList()
        });
        return View(content);
    }

    [HttpGet("{slug}")]
    [OutputCache(PolicyName = "content")]
    public IActionResult Profile(string slug)
    {
        var advocate = content.FindAdvocate(slug);
        if (advocate is null) return NotFound();

        ViewData.SetSeo(new SeoMeta
        {
            Title = $"{advocate.Name}, Advocate | Dharmacorpus",
            Description = $"{advocate.Name}, {advocate.Titles} at Dharmacorpus, a legal practice in Dharmapuri, Tamil Nadu.",
            Path = $"/team/{advocate.Slug}",
            OgType = "profile",
            NavKey = "team",
            Breadcrumbs = [("Team", "/team"), (advocate.Name, $"/team/{advocate.Slug}")],
            JsonLd = [seo.Person(advocate)]
        });

        var others = content.Advocates.Where(a => a.Slug != advocate.Slug).ToList();
        return View(new AdvocateProfileViewModel(advocate, others, content.Site));
    }
}

public sealed record AdvocateProfileViewModel(Advocate Advocate, IReadOnlyList<Advocate> Others, SiteInfo Site);
