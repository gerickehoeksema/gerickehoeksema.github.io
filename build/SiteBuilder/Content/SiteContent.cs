namespace SiteBuilder.Content;

/// <summary>Everything loaded from <c>content/</c>, before any visibility rules are applied.</summary>
public sealed record SiteContent
{
    /// <summary>From profile.json. Null only when the file is missing (the build then fails if the page needs it).</summary>
    public Profile? Profile { get; init; }

    public IReadOnlyList<ProfileLink> Links { get; init; } = [];

    /// <summary>Rendered HTML of intro.md (Home page).</summary>
    public string IntroHtml { get; init; } = string.Empty;

    /// <summary>Rendered HTML of about.md (Professional Background).</summary>
    public string AboutHtml { get; init; } = string.Empty;

    public IReadOnlyList<Skill> Skills { get; init; } = [];

    public IReadOnlyList<CompetencyGroup> Competencies { get; init; } = [];

    public IReadOnlyList<ExperienceItem> Experience { get; init; } = [];

    public IReadOnlyList<Certification> Certifications { get; init; } = [];

    public IReadOnlyList<Project> Projects { get; init; } = [];

    public IReadOnlyList<EducationItem> Education { get; init; } = [];

    public IReadOnlyList<Article> Articles { get; init; } = [];
}
