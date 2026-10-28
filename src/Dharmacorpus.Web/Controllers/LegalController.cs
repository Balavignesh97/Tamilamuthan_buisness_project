using Dharmacorpus.Web.Models;
using Dharmacorpus.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Dharmacorpus.Web.Controllers;

public sealed class LegalController(ContentService content) : Controller
{
    [HttpGet("disclaimer")]
    [OutputCache(PolicyName = "content")]
    public IActionResult Disclaimer() => Page("Disclaimer", "/disclaimer",
        "Disclaimer for the Dharmacorpus website: the site provides general information about the practice and does not constitute legal advice.");

    [HttpGet("privacy-policy")]
    [OutputCache(PolicyName = "content")]
    public IActionResult Privacy() => Page("Privacy Policy", "/privacy-policy",
        "How the Dharmacorpus website handles information you share through enquiries, and what it does not collect.");

    [HttpGet("terms-of-use")]
    [OutputCache(PolicyName = "content")]
    public IActionResult Terms() => Page("Terms of Use", "/terms-of-use",
        "Terms that apply to using the Dharmacorpus website.");

    private ViewResult Page(string title, string path, string description)
    {
        ViewData.SetSeo(new SeoMeta
        {
            Title = $"{title} | Dharmacorpus",
            Description = description,
            Path = path,
            Breadcrumbs = [(title, path)]
        });
        return View(content.Site);
    }
}
