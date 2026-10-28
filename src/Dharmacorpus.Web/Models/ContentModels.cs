namespace Dharmacorpus.Web.Models;

public sealed record SiteInfo
{
    public required string Name { get; init; }
    public required string Tagline { get; init; }
    public required string Description { get; init; }
    public required string Phone { get; init; }
    public required string PhoneDisplay { get; init; }
    public required string Email { get; init; }
    public required string Locality { get; init; }
    public required string Region { get; init; }
    public required string Country { get; init; }
    public required string CountryName { get; init; }
    public string StreetAddress { get; init; } = "";
    public string PostalCode { get; init; } = "";
    public string OpeningHours { get; init; } = "";
    public required string YearsCombined { get; init; }
    public int AdvocateCount { get; init; }
    public int PracticeAreaCount { get; init; }
}

public sealed record PracticeArea
{
    public required string Slug { get; init; }
    public required string Group { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public IReadOnlyList<string> Scope { get; init; } = [];
    public IReadOnlyList<string> WhoFor { get; init; } = [];
    public string? Prepare { get; init; }
    public IReadOnlyList<string> Related { get; init; } = [];

    /// <summary>1-based position, shown as the § number.</summary>
    public int Number { get; set; }
    public string NumberLabel => Number.ToString("00");
}

public sealed record Advocate
{
    public required string Slug { get; init; }
    public required string Name { get; init; }
    public required string Initials { get; init; }
    public required string Role { get; init; }
    public required string Titles { get; init; }
    public string? LinkedIn { get; init; }
    public string? Photo { get; init; }
}

public sealed record Faq(string Question, string Answer);
