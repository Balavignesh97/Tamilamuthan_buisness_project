using System.Net;
using System.Net.Mail;
using System.Text;
using Dharmacorpus.Web.Infrastructure;
using Dharmacorpus.Web.Models;
using Microsoft.Extensions.Options;

namespace Dharmacorpus.Web.Services;

public enum EnquiryResult { Sent, NotConfigured, Failed }

/// <summary>
/// Emails a website enquiry to the office. Nothing is stored on the server. If mail is not
/// configured the visitor is told to call or email instead, so no enquiry is silently lost.
/// </summary>
public sealed class EnquiryService(IOptions<SmtpOptions> smtpOptions, ILogger<EnquiryService> logger)
{
    private readonly SmtpOptions _smtp = smtpOptions.Value;

    public bool IsEnabled => _smtp.IsConfigured;

    public async Task<EnquiryResult> SendAsync(EnquiryForm form, CancellationToken ct)
    {
        if (!_smtp.IsConfigured)
        {
            logger.LogWarning("Enquiry received but SMTP is not configured; visitor asked to call or email.");
            return EnquiryResult.NotConfigured;
        }

        var body = new StringBuilder()
            .AppendLine("New enquiry from the Dharmacorpus website")
            .AppendLine()
            .AppendLine($"Name:      {form.Name}")
            .AppendLine($"Phone:     {form.Phone}")
            .AppendLine($"Email:     {form.Email}")
            .AppendLine($"Area:      {form.Area}")
            .AppendLine($"Language:  {form.Language}")
            .AppendLine()
            .AppendLine(form.Message)
            .ToString();

        using var message = new MailMessage(_smtp.From!, _smtp.To!)
        {
            // Strip line breaks so a visitor cannot inject extra mail headers.
            Subject = $"Website enquiry: {Sanitize(form.Area)} — {Sanitize(form.Name)}",
            Body = body,
            IsBodyHtml = false
        };
        if (!string.IsNullOrWhiteSpace(form.Email) && MailAddress.TryCreate(form.Email, out var replyTo))
            message.ReplyToList.Add(replyTo);

        using var client = new SmtpClient(_smtp.Host, _smtp.Port) { EnableSsl = _smtp.EnableSsl };
        if (!string.IsNullOrWhiteSpace(_smtp.UserName))
            client.Credentials = new NetworkCredential(_smtp.UserName, _smtp.Password);

        try
        {
            await client.SendMailAsync(message, ct);
            logger.LogInformation("Enquiry emailed (area: {Area}).", Sanitize(form.Area));
            return EnquiryResult.Sent;
        }
        catch (Exception ex) when (ex is SmtpException or InvalidOperationException)
        {
            logger.LogError(ex, "Failed to send enquiry email.");
            return EnquiryResult.Failed;
        }
    }

    private static string Sanitize(string? value) =>
        (value ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim();
}
