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

        var c = context.Content;
        context.Log($"  {c.Articles.Count} article(s), {c.Experience.Count} role(s), {c.Certifications.Count} certification(s), {c.Projects.Count} project(s)");
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
        context.ListedCertifications = Filter(context.Content.Certifications, policy.Listed);
        context.ListedProjects = Filter(context.Content.Projects, policy.Listed);

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

/// <summary>
/// Every certificate that points at a file in the repository must exist, so a renamed PDF
/// fails the build instead of becoming a broken link.
/// </summary>
public sealed class CheckLocalLinksStep : IBuildStep
{
    public string Name => "Check local links";

    public void Execute(BuildContext context)
    {
        var missing = context.ListedCertifications
            .Where(c => Rendering.Links.IsLocal(c.Link))
            .Where(c => !File.Exists(Path.Combine(context.Options.RootDirectory, c.Link)))
            .Select(c => $"{c.Id} -> {c.Link}")
            .ToList();

        if (missing.Count > 0)
        {
            throw new BuildException($"Certificate file(s) not found: {string.Join("; ", missing)}");
        }
    }
}
