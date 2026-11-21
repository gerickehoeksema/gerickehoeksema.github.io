using SiteBuilder.Content;

namespace SiteBuilder.Specifications;

/// <summary>A single, testable rule about a content item (Specification pattern).</summary>
public interface ISpecification<in T>
{
    bool IsSatisfiedBy(T item);
}

/// <summary>Composition: <c>new NotDraft().And(new PublishedBy(today))</c>.</summary>
public static class Specification
{
    public static ISpecification<T> And<T>(this ISpecification<T> left, ISpecification<T> right) =>
        new Lambda<T>(x => left.IsSatisfiedBy(x) && right.IsSatisfiedBy(x));

    public static ISpecification<T> Or<T>(this ISpecification<T> left, ISpecification<T> right) =>
        new Lambda<T>(x => left.IsSatisfiedBy(x) || right.IsSatisfiedBy(x));

    public static ISpecification<T> Not<T>(this ISpecification<T> spec) =>
        new Lambda<T>(x => !spec.IsSatisfiedBy(x));

    public static ISpecification<T> All<T>() => new Lambda<T>(_ => true);

    private sealed class Lambda<T>(Func<T, bool> predicate) : ISpecification<T>
    {
        public bool IsSatisfiedBy(T item) => predicate(item);
    }
}

/// <summary>Drafts are never published.</summary>
public sealed class NotDraft : ISpecification<ContentItem>
{
    public bool IsSatisfiedBy(ContentItem item) => item.Visibility != Visibility.Draft;
}

/// <summary>Scheduled items appear on or after their <c>publishFrom</c> date.</summary>
public sealed class PublishedBy(DateOnly today) : ISpecification<ContentItem>
{
    public bool IsSatisfiedBy(ContentItem item) => item.PublishFrom is null || item.PublishFrom <= today;
}

/// <summary>Unlisted items are reachable by link but stay out of lists and the sitemap.</summary>
public sealed class Unlisted : ISpecification<ContentItem>
{
    public bool IsSatisfiedBy(ContentItem item) => item.Visibility == Visibility.Unlisted;
}

/// <summary>Items that belong to a lens (ready for the Work / Life switch).</summary>
public sealed class InLens(Lens lens) : ISpecification<ContentItem>
{
    public bool IsSatisfiedBy(ContentItem item) => item.Lenses.Contains(lens);
}
