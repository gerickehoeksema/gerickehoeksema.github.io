namespace SiteBuilder.Content;

/// <summary>Everything loaded from <c>content/</c>, before any visibility rules are applied.</summary>
public sealed record SiteContent
{
    public IReadOnlyList<Article> Articles { get; init; } = [];

    public IReadOnlyList<ExperienceItem> Experience { get; init; } = [];

    public IReadOnlyList<Certification> Certifications { get; init; } = [];

    public IReadOnlyList<Project> Projects { get; init; } = [];
}
