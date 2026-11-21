namespace SiteBuilder.Content;

/// <summary>A tile under "Skills &amp; Expertise", from <c>content/skills.json</c>.</summary>
public sealed record Skill : ContentItem
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    /// <summary>Icon id from the sprite in index.html.</summary>
    public required string Icon { get; init; }
}

/// <summary>A group under "Core Competencies", from <c>content/competencies.json</c>.</summary>
public sealed record CompetencyGroup : ContentItem
{
    public required string Title { get; init; }

    public IReadOnlyList<string> Items { get; init; } = [];
}

/// <summary>A school or course, from <c>content/education.json</c>. Listed in file order.</summary>
public sealed record EducationItem : ContentItem
{
    public required string School { get; init; }

    public required string Qualification { get; init; }

    /// <summary>Leave out for a single year.</summary>
    public int? Start { get; init; }

    public required int End { get; init; }

    public string Years => Start is { } start && start != End ? $"{start} – {End}" : End.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
