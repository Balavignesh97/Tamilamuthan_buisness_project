using System.ComponentModel.DataAnnotations;

namespace Dharmacorpus.Web.Models;

public sealed class EnquiryForm : IValidatableObject
{
    public static readonly string[] Languages = ["English", "Tamil"];

    [Required(ErrorMessage = "Please enter your name.")]
    [StringLength(100, ErrorMessage = "Please keep your name under 100 characters.")]
    [Display(Name = "Full name")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Please enter a phone number.")]
    [RegularExpression(@"^\+?[0-9\s\-]{10,16}$", ErrorMessage = "Enter a valid phone number, e.g. +91 98765 43210.")]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Please choose the area closest to your matter.")]
    [Display(Name = "Area of your matter")]
    public string? Area { get; set; }

    [Display(Name = "Preferred language")]
    public string Language { get; set; } = "English";

    [Required(ErrorMessage = "Please describe your matter briefly.")]
    [StringLength(600, MinimumLength = 10, ErrorMessage = "Please write between 10 and 600 characters.")]
    [Display(Name = "Briefly describe your matter")]
    public string? Message { get; set; }

    [Display(Name = "Consent")]
    public bool Consent { get; set; }

    /// <summary>Honeypot: hidden from people, filled in by bots. Must stay empty.</summary>
    public string? Website { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Consent)
            yield return new ValidationResult("Please confirm you have read this note.", [nameof(Consent)]);
        if (!Languages.Contains(Language))
            yield return new ValidationResult("Choose English or Tamil.", [nameof(Language)]);
    }
}

public sealed class ContactViewModel
{
    public EnquiryForm Form { get; init; } = new();
    public IReadOnlyList<string> Areas { get; init; } = [];
    public bool Sent { get; init; }
    public string? Error { get; init; }
}
