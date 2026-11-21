namespace SiteBuilder.Content;

/// <summary>A long-form article, authored as <c>content/articles/&lt;id&gt;/index.md</c>.</summary>
public sealed record Article : ContentItem
{
    private const int WordsPerMinute = 200;

    public required string Title { get; init; }

    /// <summary>One sentence for search results, share cards and the article list.</summary>
    public required string Description { get; init; }

    /// <summary>The intro paragraph under the title. Falls back to <see cref="Description"/>.</summary>
    public string? Lede { get; init; }

    /// <summary>Small label above the title, e.g. "Architecture".</summary>
    public string? Kicker { get; init; }

    /// <summary>Icon id from the sprite in index.html, used on the article card.</summary>
    public string Icon { get; init; } = "pen-line";

    public required DateOnly Date { get; init; }

    public IReadOnlyList<string> Keywords { get; init; } = [];

    /// <summary>Rendered HTML of the Markdown body.</summary>
    public required string BodyHtml { get; init; }

    public required int WordCount { get; init; }

    /// <summary>Folder the article was loaded from; anything next to index.md is copied as an asset.</summary>
    public required string SourceDirectory { get; init; }

    public int ReadingMinutes => Math.Max(1, (int)Math.Round(WordCount / (double)WordsPerMinute));

    public string Path => $"/articles/{Id}/";
}
