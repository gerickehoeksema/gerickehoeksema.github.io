using SiteBuilder.Content;
using SiteBuilder.Specifications;

namespace SiteBuilder.Pipeline.Steps;

public sealed class LoadContentStep : IBuildStep
{
    public string Name => "Load content";

    public void Execute(BuildContext context)
    {
        try
        {
            context.Content = new ContentLoader().Load(context.Options.ContentDirectory);
        }
        catch (ContentException ex)
        {
            throw new BuildException(ex.Message);
        }

        context.Log($"  {context.Content.Articles.Count} article(s), {context.Content.Experience.Count} role(s)");
    }
}

/// <summary>Filters content once, so later steps can't forget to.</summary>
public sealed class ApplyVisibilityStep : IBuildStep
{
    public string Name => "Apply visibility rules";

    public void Execute(BuildContext context)
    {
        var policy = context.Options.Policy;

        context.RenderedArticles = Filter(context.Content.Articles, policy.Rendered);
        context.ListedArticles = Filter(context.Content.Articles, policy.Listed);
        context.ListedExperience = Filter(context.Content.Experience, policy.Listed);

        foreach (var skipped in context.Content.Articles.Except(context.RenderedArticles))
        {
            var reason = skipped.Visibility == Visibility.Draft ? "draft" : $"scheduled for {skipped.PublishFrom:yyyy-MM-dd}";
            context.Log($"  skipped article \"{skipped.Id}\" ({reason})");
        }

        if (context.Options.Preview)
        {
            context.Log("  PREVIEW: drafts and scheduled items are included. Do not deploy this output.");
        }
    }

    private static IReadOnlyList<T> Filter<T>(IEnumerable<T> items, ISpecification<ContentItem> spec) where T : ContentItem =>
        items.Where(spec.IsSatisfiedBy).ToList();
}

/// <summary>
/// Stops a half-finished article going live: anything published must not contain draft
/// placeholders (<c>&lt;mark class="todo"&gt;</c>).
/// </summary>
public sealed class PublishGuardStep : IBuildStep
{
    internal const string TodoMarker = "class=\"todo\"";

    public string Name => "Check published content for draft placeholders";

    public void Execute(BuildContext context)
    {
        var offenders = context.RenderedArticles
            .Where(a => a.Visibility != Visibility.Draft && a.BodyHtml.Contains(TodoMarker, StringComparison.Ordinal))
            .Select(a => a.Id)
            .ToList();

        if (offenders.Count > 0)
        {
            throw new BuildException(
                $"Published article(s) still contain draft placeholders (<mark class=\"todo\">): {string.Join(", ", offenders)}. " +
                "Fill them in, or set visibility back to draft.");
        }
    }
}
