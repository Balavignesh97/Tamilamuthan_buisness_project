using Dharmacorpus.Web.Models;
using Dharmacorpus.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dharmacorpus.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public sealed class ErrorController(ContentService content) : Controller
{
    [Route("error/{code:int?}")]
    public IActionResult Index(int? code)
    {
        var status = code ?? 500;
        Response.StatusCode = status;
        ViewData.SetSeo(new SeoMeta
        {
            Title = status == 404 ? "Page not found | Dharmacorpus" : "Something went wrong | Dharmacorpus",
            Description = "This page could not be shown.",
            Path = "/",
            NoIndex = true
        });
        ViewData["StatusCode"] = status;
        return View(content);
    }
}
