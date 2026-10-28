using Dharmacorpus.Web.Models;
using Dharmacorpus.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dharmacorpus.Web.Controllers;

[Route("contact")]
public sealed class ContactController(ContentService content, EnquiryService enquiries) : Controller
{
    private const string NotConfiguredMessage =
        "Online enquiries are not switched on yet. Please call or email the office directly.";
    private const string FailedMessage =
        "Sorry, your enquiry could not be sent just now. Please call or email the office directly.";

    [HttpGet("")]
    public IActionResult Index()
    {
        SetSeo();
        return View(BuildModel(new EnquiryForm()));
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("enquiry")]
    public async Task<IActionResult> Index([Bind(Prefix = "Form")] EnquiryForm form, CancellationToken ct)
    {
        var isAjax = Request.Headers.XRequestedWith == "XMLHttpRequest";

        if (!content.PracticeAreas.Any(p => p.Title == form.Area) && form.Area != "Not sure")
            ModelState.AddModelError("Form.Area", "Please choose the area closest to your matter.");

        // Bots fill the hidden field: pretend success, send nothing.
        if (!string.IsNullOrEmpty(form.Website))
            return isAjax ? Json(new { ok = true }) : RedirectToAction(nameof(Thanks));

        if (!ModelState.IsValid)
        {
            if (isAjax)
            {
                var errors = ModelState.Where(kv => kv.Value!.Errors.Count > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value!.Errors.First().ErrorMessage);
                return BadRequest(new { ok = false, errors });
            }
            SetSeo();
            return View(BuildModel(form));
        }

        var result = await enquiries.SendAsync(form, ct);
        var error = result switch
        {
            EnquiryResult.Sent => null,
            EnquiryResult.NotConfigured => NotConfiguredMessage,
            _ => FailedMessage
        };

        if (isAjax)
            return error is null ? Json(new { ok = true }) : StatusCode(503, new { ok = false, message = error });

        if (error is null) return RedirectToAction(nameof(Thanks));
        SetSeo();
        return View(BuildModel(form, error: error));
    }

    [HttpGet("thank-you")]
    public IActionResult Thanks()
    {
        SetSeo(noIndex: true);
        return View("Index", BuildModel(new EnquiryForm(), sent: true));
    }

    private ContactViewModel BuildModel(EnquiryForm form, bool sent = false, string? error = null) => new()
    {
        Form = form,
        Areas = content.PracticeAreas.Select(p => p.Title).Append("Not sure").ToList(),
        Sent = sent,
        Error = error
    };

    private void SetSeo(bool noIndex = false) => ViewData.SetSeo(new SeoMeta
    {
        Title = "Contact | Advocates in Dharmapuri | Dharmacorpus",
        Description = "Contact Dharmacorpus in Dharmapuri, Tamil Nadu, to discuss a legal matter and arrange a consultation.",
        Path = "/contact",
        NavKey = "contact",
        NoIndex = noIndex,
        Breadcrumbs = [("Contact", "/contact")]
    });
}
