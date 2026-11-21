using SiteBuilder.Content;

namespace SiteBuilder.Specifications;

/// <summary>
/// The two questions the build asks of every item, composed from the rules in <see cref="Specification"/>.
/// </summary>
/// <param name="Today">The build date. Scheduled items appear once this reaches their publishFrom.</param>
/// <param name="Preview">Local preview: drafts and scheduled items are rendered too (never use for deploys).</param>
public sealed record VisibilityPolicy(DateOnly Today, bool Preview)
{
    /// <summary>Is the item written to the output at all?</summary>
    public ISpecification<ContentItem> Rendered => Preview
        ? Specification.All<ContentItem>()
        : new NotDraft().And(new PublishedBy(Today));

    /// <summary>Does the item appear in lists, on the home page and in the sitemap?</summary>
    public ISpecification<ContentItem> Listed => Rendered.And(new Unlisted().Not());
}
