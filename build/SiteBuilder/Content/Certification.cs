namespace SiteBuilder.Content;

/// <summary>A certificate, from <c>content/certifications.json</c>. Listed in file order.</summary>
public sealed record Certification : ContentItem
{
    public required string Title { get; init; }

    public required string Issuer { get; init; }

    /// <summary>
    /// The certificate: a site-relative file such as <c>certifications/Azure Fundamentals.pdf</c>
    /// (written as-is, the builder encodes it), or a full https:// URL to a verification page.
    /// Local files must exist, or the build fails.
    /// </summary>
    public required string Link { get; init; }

    public string LinkText { get; init; } = "View certificate";
}
