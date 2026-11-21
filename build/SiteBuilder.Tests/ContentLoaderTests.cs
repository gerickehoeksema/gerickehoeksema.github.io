using SiteBuilder.Content;
using SiteBuilder.Pipeline;
using SiteBuilder.Rendering;

namespace SiteBuilder.Tests;

public class ContentLoaderTests
{
    [Fact]
    public void Parses_front_matter_and_renders_markdown()
    {
        using var site = new TestSite().WithArticle("hello", """
            title: "Hello"
            description: "A test"
            kicker: Architecture
            icon: boxes
            date: 2026-10-05
            lens: [work, life]
            visibility: unlisted
            publishFrom: 2026-10-06
            keywords: [a, b]
            """, "## Intro\n\nSome *text*.\n\n{.checklist}\n1. One");

        var article = new ContentLoader().Load(Path.Combine(site.Root, "content")).Articles.Single();

        Assert.Equal("hello", article.Id);
        Assert.Equal("Hello", article.Title);
        Assert.Equal("Architecture", article.Kicker);
        Assert.Equal(new DateOnly(2026, 10, 5), article.Date);
        Assert.Equal([Lens.Work, Lens.Life], article.Lenses);
        Assert.Equal(Visibility.Unlisted, article.Visibility);
        Assert.Equal(new DateOnly(2026, 10, 6), article.PublishFrom);
        Assert.Contains("<h2 id=\"intro\">Intro</h2>", article.BodyHtml);
        Assert.Contains("<ol class=\"checklist\">", article.BodyHtml);
        Assert.DoesNotContain("title:", article.BodyHtml); // front matter is not rendered
    }

    [Fact]
    public void Defaults_to_public_in_the_work_lens()
    {
        using var site = new TestSite().WithArticle("plain", "title: T\ndescription: D\ndate: 2026-01-01");

        var article = new ContentLoader().Load(Path.Combine(site.Root, "content")).Articles.Single();

        Assert.Equal(Visibility.Public, article.Visibility);
        Assert.Equal([Lens.Work], article.Lenses);
    }

    [Theory]
    [InlineData("title: T\ndescription: D\ndate: 2026-01-01\nvisiblity: draft", "visiblity")]  // typo must not silently publish
    [InlineData("title: T\ndescription: D\ndate: 2026-01-01\nvisibility: secret", "secret")]
    [InlineData("title: T\ndescription: D\ndate: 01/02/2026", "yyyy-MM-dd")]
    [InlineData("description: D\ndate: 2026-01-01", "title")]
    [InlineData("id: other\ntitle: T\ndescription: D\ndate: 2026-01-01", "folder name")]
    public void Rejects_invalid_front_matter_with_the_file_name(string frontMatter, string expected)
    {
        using var site = new TestSite().WithArticle("bad", frontMatter);

        var ex = Assert.Throws<ContentException>(() => new ContentLoader().Load(Path.Combine(site.Root, "content")));

        Assert.Contains("index.md", ex.Message);
        Assert.Contains(expected, ex.Message);
    }

    [Fact]
    public void Experience_json_is_strict_about_field_names()
    {
        using var site = new TestSite().WithExperience("""[{ "id": "a", "role": "R", "company": "C", "start": "2020-01", "summary": "S", "visiblity": "draft" }]""");

        var ex = Assert.Throws<ContentException>(() => new ContentLoader().Load(Path.Combine(site.Root, "content")));
        Assert.Contains("visiblity", ex.Message);
    }

    [Fact]
    public void Experience_is_sorted_newest_first_and_formats_dates()
    {
        using var site = new TestSite().WithExperience("""
            [
              { "id": "old", "role": "R1", "company": "C1", "start": "2015-11", "end": "2018-01", "summary": "S" },
              { "id": "now", "role": "R2", "company": "C2", "start": "2021-08", "summary": "S", "lens": ["work"] }
            ]
            """);

        var roles = new ContentLoader().Load(Path.Combine(site.Root, "content")).Experience;

        Assert.Equal(["now", "old"], roles.Select(r => r.Id));
        Assert.Equal("Aug 2021 – Present", HomeSections.DateRange(roles[0]));
        Assert.Equal("Nov 2015 – Jan 2018", HomeSections.DateRange(roles[1]));
    }

    [Fact]
    public void The_real_content_folder_loads()
    {
        var root = FindRepositoryRoot();

        var content = new ContentLoader().Load(Path.Combine(root, "content"));

        Assert.NotEmpty(content.Experience);
        Assert.All(content.Articles, a => Assert.False(string.IsNullOrWhiteSpace(a.BodyHtml)));
    }

    internal static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "content")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new BuildException("Repository root (with content/) not found.");
    }
}

public class HtmlTests
{
    [Theory]
    [InlineData("Plain · text – ok", "Plain · text – ok")]
    [InlineData("<script>\"a\" & 'b'</script>", "&lt;script&gt;&quot;a&quot; &amp; &#39;b&#39;&lt;/script&gt;")]
    [InlineData(null, "")]
    public void E_escapes_only_html_special_characters(string? input, string expected) =>
        Assert.Equal(expected, SiteBuilder.Rendering.Html.E(input));

    [Fact]
    public void Regions_replace_content_and_keep_markers_and_indentation()
    {
        var html = "<div>\n    <!-- build:x · note -->\n    <p>old</p>\n    <!-- /build:x -->\n</div>";

        var result = SiteBuilder.Rendering.Regions.Replace(html, "x", "<p>new</p>\n<p>two</p>");

        Assert.Equal("<div>\n    <!-- build:x · note -->\n    <p>new</p>\n    <p>two</p>\n    <!-- /build:x -->\n</div>", result);
    }
}
