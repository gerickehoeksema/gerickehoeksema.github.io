using SiteBuilder.Pipeline;

namespace SiteBuilder.Tests;

/// <summary>A throwaway site on disk: a minimal index.html with build regions, plus content.</summary>
internal sealed class TestSite : IDisposable
{
    public static readonly DateOnly Today = new(2026, 11, 21);

    private const string IndexHtml = """
        <!DOCTYPE html>
        <html><head>
            <!-- build:meta --><title>fallback title</title><!-- /build:meta -->
            <!-- build:social --><!-- /build:social -->
            <!-- build:jsonld --><!-- /build:jsonld -->
        </head><body>
            <div class="profile-info">
                <!-- build:profile --><h1>fallback name</h1><!-- /build:profile -->
            </div>
            <div class="profile-actions">
                <!-- build:links --><!-- /build:links -->
            </div>
            <div class="card-content">
                <!-- build:intro --><!-- /build:intro -->
            </div>
            <div class="skills-container">
                <!-- build:skills --><!-- /build:skills -->
            </div>
            <div class="card-content">
                <!-- build:about --><!-- /build:about -->
                <div class="competencies-grid">
                    <!-- build:competencies --><!-- /build:competencies -->
                </div>
            </div>
            <div class="education">
                <!-- build:education --><!-- /build:education -->
            </div>
            <footer>
                <!-- build:copyright --><p>fallback copyright</p><!-- /build:copyright -->
            </footer>
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
        WithJson("profile.json", DefaultProfile);
    }

    public const string DefaultProfile = """
        {
          "name": "Test Person",
          "givenName": "Test",
          "familyName": "Person",
          "jobTitle": "Software Architect",
          "headline": "Builder of things",
          "location": { "locality": "Jeffreys Bay", "country": "South Africa", "countryCode": "ZA" },
          "description": "Search description",
          "shareDescription": "Share description",
          "image": "images/profile-600.webp",
          "knowsAbout": ["C#"],
          "sameAs": ["https://github.com/example"]
        }
        """;

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

    public TestSite WithText(string fileName, string text)
    {
        File.WriteAllText(Path.Combine(Root, "content", fileName), text);
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
