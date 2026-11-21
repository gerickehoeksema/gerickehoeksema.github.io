namespace SiteBuilder.Content;

/// <summary>
/// Who the site is about, from <c>content/profile.json</c>. Drives the profile header and the
/// page metadata (&lt;title&gt;, description, share card, JSON-LD), so the job title lives in one place.
/// </summary>
public sealed record Profile
{
    public required string Name { get; init; }

    public required string GivenName { get; init; }

    public required string FamilyName { get; init; }

    /// <summary>Used in the page title, share title and JSON-LD.</summary>
    public required string JobTitle { get; init; }

    /// <summary>The line under the name in the header.</summary>
    public required string Headline { get; init; }

    public required ProfileLocation Location { get; init; }

    /// <summary>Search-result description (meta description).</summary>
    public required string Description { get; init; }

    /// <summary>Description on LinkedIn / WhatsApp share cards.</summary>
    public required string ShareDescription { get; init; }

    /// <summary>Site-relative portrait used in JSON-LD.</summary>
    public required string Image { get; init; }

    public IReadOnlyList<string> KnowsAbout { get; init; } = [];

    /// <summary>Other profiles that are you (JSON-LD sameAs), for search engines.</summary>
    public IReadOnlyList<string> SameAs { get; init; } = [];

    public string Title => $"{Name} - {JobTitle}";
}

public sealed record ProfileLocation
{
    public required string Locality { get; init; }

    public required string Country { get; init; }

    /// <summary>ISO 3166 code for JSON-LD, e.g. "ZA".</summary>
    public required string CountryCode { get; init; }

    public string Display => $"{Locality}, {Country}";
}

/// <summary>A button under the profile header, from <c>content/links.json</c>. Listed in file order.</summary>
public sealed record ProfileLink : ContentItem
{
    public required string Label { get; init; }

    public required string Url { get; init; }

    /// <summary>Icon id from the sprite in index.html.</summary>
    public required string Icon { get; init; }

    /// <summary>Brand colour for the icon (a <c>brand-*</c> class), e.g. "linkedin".</summary>
    public string? Brand { get; init; }

    /// <summary>Shown as the filled amber button (the main call to action).</summary>
    public bool Primary { get; init; }
}
