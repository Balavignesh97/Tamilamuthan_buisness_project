using Dharmacorpus.Web.Models;
using Dharmacorpus.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Dharmacorpus.Web.Controllers;

public sealed class HomeController(ContentService content, SeoService seo) : Controller
{
    [HttpGet("")]
    [OutputCache(PolicyName = "content")]
    public IActionResult Index()
    {
        ViewData.SetSeo(new SeoMeta
        {
            Title = "Dharmacorpus | Advocates & Legal Practice in Dharmapuri, Tamil Nadu",
            Description = content.Site.Description,
            Path = "/",
            NavKey = "home",
            JsonLd = [seo.FaqPage(content.Faqs, "/")]
        });
        return View(content);
    }

    [HttpGet("about")]
    [OutputCache(PolicyName = "content")]
    public IActionResult About()
    {
        ViewData.SetSeo(new SeoMeta
        {
            Title = "About | Dharmacorpus",
            Description = "About Dharmacorpus, a legal practice based in Dharmapuri, Tamil Nadu, providing rigorous, accessible and practical legal assistance.",
            Path = "/about",
            NavKey = "about",
            Breadcrumbs = [("About", "/about")]
        });
        return View(content);
    }
}
