using System.Text.Json.Serialization;

namespace SiteBuilder.Content;

/// <summary>Which side of the site an item belongs to. Drives the Work / All / Life lens (PLAN-v3 §3.1).</summary>
public enum Lens
{
    Work,
    Life,
}

/// <summary>Who gets to see an item (PLAN-v3 §2).</summary>
public enum Visibility
{
    /// <summary>Rendered and listed.</summary>
    Public,

    /// <summary>Rendered, reachable by direct link, but never listed or added to the sitemap.</summary>
    Unlisted,

    /// <summary>Never written to the output, except in a local preview build.</summary>
    Draft,
}

/// <summary>
/// The visibility contract every content type shares. Visibility rules
/// (see <c>Specifications</c>) only ever look at these members.
/// </summary>
public abstract record ContentItem
{
    public required string Id { get; init; }

    /// <summary>Written as "lens" in content files, matching PLAN-v3 §2.</summary>
    [JsonPropertyName("lens")]
    public IReadOnlyList<Lens> Lenses { get; init; } = [Lens.Work];

    public Visibility Visibility { get; init; } = Visibility.Public;

    /// <summary>Optional scheduled reveal: hidden until this date.</summary>
    public DateOnly? PublishFrom { get; init; }

    public bool Featured { get; init; }
}
