using SiteBuilder.Content;
using SiteBuilder.Specifications;

namespace SiteBuilder.Pipeline;

/// <param name="RootDirectory">Repository root: holds the static site files and <c>content/</c>.</param>
/// <param name="OutputDirectory">Where the finished site is written (normally <c>dist/</c>).</param>
/// <param name="Today">Build date, used for scheduled content.</param>
/// <param name="Preview">Render drafts and scheduled items, for local review only.</param>
public sealed record BuildOptions(
    string RootDirectory,
    string OutputDirectory,
    DateOnly Today,
    bool Preview = false,
    string SiteUrl = "https://gerickehoeksema.co.za")
{
    public string ContentDirectory => Path.Combine(RootDirectory, "content");

    public VisibilityPolicy Policy => new(Today, Preview);
}

/// <summary>A build failure caused by the content, not a bug. Reported without a stack trace.</summary>
public sealed class BuildException(string message) : Exception(message);

/// <summary>State passed along the pipeline. Each step reads what earlier steps produced.</summary>
public sealed class BuildContext(BuildOptions options, Action<string> log)
{
    public BuildOptions Options { get; } = options;

    public Action<string> Log { get; } = log;

    /// <summary>Set by <c>LoadContentStep</c>.</summary>
    public SiteContent Content { get; set; } = new([], []);

    /// <summary>Set by <c>ApplyVisibilityStep</c>: articles written to the output.</summary>
    public IReadOnlyList<Article> RenderedArticles { get; set; } = [];

    /// <summary>Set by <c>ApplyVisibilityStep</c>: articles shown in lists and the sitemap.</summary>
    public IReadOnlyList<Article> ListedArticles { get; set; } = [];

    /// <summary>Set by <c>ApplyVisibilityStep</c>.</summary>
    public IReadOnlyList<ExperienceItem> ListedExperience { get; set; } = [];

    public string OutputPath(params string[] parts) => Path.Combine([Options.OutputDirectory, .. parts]);
}

/// <summary>One stage of the build. New stages slot in without touching the others.</summary>
public interface IBuildStep
{
    string Name { get; }

    void Execute(BuildContext context);
}

/// <summary>Runs the steps in order (Pipeline pattern).</summary>
public sealed class BuildPipeline(IEnumerable<IBuildStep> steps)
{
    private readonly IReadOnlyList<IBuildStep> _steps = steps.ToList();

    /// <summary>The standard site build.</summary>
    public static BuildPipeline Default() => new(
    [
        new Steps.LoadContentStep(),
        new Steps.ApplyVisibilityStep(),
        new Steps.PublishGuardStep(),
        new Steps.CopyStaticFilesStep(),
        new Steps.RenderArticlesStep(),
        new Steps.RenderHomeStep(),
        new Steps.WriteSitemapStep(),
    ]);

    public BuildContext Run(BuildOptions options, Action<string>? log = null)
    {
        var context = new BuildContext(options, log ?? (_ => { }));
        foreach (var step in _steps)
        {
            context.Log($"> {step.Name}");
            step.Execute(context);
        }

        return context;
    }
}
