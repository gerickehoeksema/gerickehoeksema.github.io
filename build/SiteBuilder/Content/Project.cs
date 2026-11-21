namespace SiteBuilder.Content;

/// <summary>A project card, from <c>content/projects.json</c>. Listed in file order.</summary>
public sealed record Project : ContentItem
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Url { get; init; }

    public string LinkText { get; init; } = "Visit project";

    /// <summary>Icon id from the sprite in index.html.</summary>
    public string Icon { get; init; } = "code";

    /// <summary>Optional brand colour for the icon (a <c>brand-*</c> class in styles.css), e.g. "github".</summary>
    public string? Brand { get; init; }
}
