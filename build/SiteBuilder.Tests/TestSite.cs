using SiteBuilder.Pipeline;

namespace SiteBuilder.Tests;

/// <summary>A throwaway site on disk: a minimal index.html with build regions, plus content.</summary>
internal sealed class TestSite : IDisposable
{
    public static readonly DateOnly Today = new(2026, 11, 21);

    private const string IndexHtml = """
        <!DOCTYPE html>
        <html><body>
            <div class="timeline">
                <!-- build:experience · generated -->
                <p>fallback experience</p>
                <!-- /build:experience -->
            </div>
            <div class="certifications-list">
                <!-- build:certifications -->
                <p>fallback certifications</p>
                <!-- /build:certifications -->
            </div>
            <div class="projects-grid">
                <!-- build:projects -->
                <p>fallback projects</p>
                <!-- /build:projects -->
            </div>
            <div class="articles-grid">
                <!-- build:articles -->
                <p>fallback articles</p>
                <!-- /build:articles -->
            </div>
        </body></html>
        """;

    public TestSite()
    {
        Root = Directory.CreateTempSubdirectory("sitebuilder-").FullName;
        File.WriteAllText(Path.Combine(Root, "index.html"), IndexHtml);
        File.WriteAllText(Path.Combine(Root, "styles.css"), "body{}");
        File.WriteAllText(Path.Combine(Root, "README.md"), "source only");
        Directory.CreateDirectory(Path.Combine(Root, "content", "articles"));
    }

    public string Root { get; }

    public string Dist => Path.Combine(Root, "dist");

    public TestSite WithArticle(string id, string frontMatter, string body = "Hello **world**.")
    {
        var dir = Directory.CreateDirectory(Path.Combine(Root, "content", "articles", id)).FullName;
        File.WriteAllText(Path.Combine(dir, "index.md"), $"---\n{frontMatter.Trim()}\n---\n\n{body}\n");
        return this;
    }

    public static string FrontMatter(string title, string visibility = "public", string? publishFrom = null) =>
        $"""
        title: "{title}"
        description: "About {title}"
        date: 2026-11-01
        visibility: {visibility}
        {(publishFrom is null ? "" : $"publishFrom: {publishFrom}")}
        """;

    public TestSite WithExperience(string json) => WithJson("experience.json", json);

    public TestSite WithJson(string fileName, string json)
    {
        File.WriteAllText(Path.Combine(Root, "content", fileName), json);
        return this;
    }

    public TestSite WithFile(string relativePath, string contents = "file")
    {
        var path = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
        return this;
    }

    public BuildContext Build(bool preview = false, DateOnly? today = null, string? output = null) =>
        BuildPipeline.Default().Run(new BuildOptions(Root, output ?? Dist, today ?? Today, preview));

    public string Read(params string[] path) => File.ReadAllText(Path.Combine([Dist, .. path]));

    /// <summary>Every byte of text in the output, for "this must never appear" assertions.</summary>
    public string AllOutputText(string? output = null) =>
        string.Concat(Directory.EnumerateFiles(output ?? Dist, "*", SearchOption.AllDirectories).Select(File.ReadAllText));

    public void Dispose()
    {
        try { Directory.Delete(Root, recursive: true); } catch (IOException) { /* best effort */ }
    }
}
