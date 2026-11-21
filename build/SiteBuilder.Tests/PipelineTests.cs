using SiteBuilder.Pipeline;

namespace SiteBuilder.Tests;

/// <summary>End-to-end builds into a temp folder. These are the safety net for "what gets published".</summary>
public class PipelineTests
{
    [Fact]
    public void A_draft_is_never_written_to_the_output()
    {
        using var site = new TestSite()
            .WithArticle("secret-draft", TestSite.FrontMatter("Top secret draft", "draft"), "Unfinished thoughts.");

        site.Build();

        Assert.False(Directory.Exists(Path.Combine(site.Dist, "articles", "secret-draft")));
        Assert.DoesNotContain("Top secret draft", site.AllOutputText());
        Assert.DoesNotContain("secret-draft", site.AllOutputText());
        Assert.DoesNotContain("Unfinished thoughts", site.AllOutputText());
    }

    [Fact]
    public void A_public_article_is_rendered_listed_and_in_the_sitemap()
    {
        using var site = new TestSite().WithArticle("live", TestSite.FrontMatter("Live one"));

        site.Build();

        Assert.Contains("<h1>Live one</h1>", site.Read("articles", "live", "index.html"));
        Assert.DoesNotContain("noindex", site.Read("articles", "live", "index.html"));
        Assert.Contains("href=\"articles/live/\"", site.Read("index.html"));
        Assert.DoesNotContain("fallback articles", site.Read("index.html"));
        Assert.Contains("/articles/live/", site.Read("sitemap.xml"));
    }

    [Fact]
    public void An_unlisted_article_is_reachable_but_not_listed()
    {
        using var site = new TestSite().WithArticle("hidden", TestSite.FrontMatter("Hidden one", "unlisted"));

        site.Build();

        Assert.True(File.Exists(Path.Combine(site.Dist, "articles", "hidden", "index.html")));
        Assert.DoesNotContain("articles/hidden", site.Read("index.html"));
        Assert.DoesNotContain("/articles/hidden/", site.Read("sitemap.xml"));
    }

    [Fact]
    public void A_scheduled_article_appears_on_its_date()
    {
        using var site = new TestSite().WithArticle("later", TestSite.FrontMatter("Later one", publishFrom: "2026-12-01"));

        site.Build(today: new DateOnly(2026, 11, 30));
        Assert.DoesNotContain("Later one", site.AllOutputText());

        site.Build(today: new DateOnly(2026, 12, 1));
        Assert.Contains("Later one", site.Read("index.html"));
    }

    [Fact]
    public void Preview_renders_drafts_with_a_banner_and_noindex()
    {
        using var site = new TestSite().WithArticle("draft", TestSite.FrontMatter("Draft one", "draft"));

        site.Build(preview: true);

        var page = site.Read("articles", "draft", "index.html");
        Assert.Contains("<meta name=\"robots\" content=\"noindex\">", page);
        Assert.Contains("draft-banner", page);
        Assert.Contains("Draft · ", site.Read("index.html"));
    }

    [Fact]
    public void Shows_the_empty_state_when_nothing_is_published()
    {
        using var site = new TestSite().WithArticle("draft", TestSite.FrontMatter("Draft one", "draft"));

        site.Build();

        Assert.Contains("First article on the way", site.Read("index.html"));
    }

    [Fact]
    public void A_published_article_with_placeholders_fails_the_build()
    {
        using var site = new TestSite().WithArticle("oops", TestSite.FrontMatter("Oops"), "We used <mark class=\"todo\">[broker]</mark>.");

        var ex = Assert.Throws<BuildException>(() => site.Build());

        Assert.Contains("oops", ex.Message);
    }

    [Fact]
    public void A_draft_with_placeholders_builds_fine()
    {
        using var site = new TestSite().WithArticle("wip", TestSite.FrontMatter("WIP", "draft"), "<mark class=\"todo\">[later]</mark>");

        site.Build();
    }

    [Fact]
    public void Source_only_files_are_not_copied()
    {
        using var site = new TestSite();

        site.Build();

        Assert.True(File.Exists(Path.Combine(site.Dist, "styles.css")));
        Assert.False(File.Exists(Path.Combine(site.Dist, "README.md")));
        Assert.False(Directory.Exists(Path.Combine(site.Dist, "content")));
    }

    [Fact]
    public void An_output_folder_inside_the_root_is_not_copied_into_itself()
    {
        using var site = new TestSite();
        var output = Path.Combine(site.Root, "dist-preview");

        site.Build(output: output);
        site.Build(output: output); // second run sees the first output in the root

        Assert.False(Directory.Exists(Path.Combine(output, "dist-preview")));
    }

    [Fact]
    public void Refuses_to_write_over_the_source()
    {
        using var site = new TestSite();

        Assert.Throws<BuildException>(() => site.Build(output: site.Root));
        Assert.True(File.Exists(Path.Combine(site.Root, "index.html")));
    }

    [Fact]
    public void A_missing_region_fails_with_a_clear_message()
    {
        using var site = new TestSite();
        File.WriteAllText(Path.Combine(site.Root, "index.html"), "<html><!-- build:projects --><!-- /build:projects --></html>");

        var ex = Assert.Throws<BuildException>(() => site.Build());

        Assert.Contains("not found", ex.Message);
        Assert.Contains("<!-- build:", ex.Message); // tells you which marker to add
    }

    [Fact]
    public void The_real_site_builds_and_its_draft_stays_out()
    {
        var root = ContentLoaderTests.FindRepositoryRoot();
        var output = Directory.CreateTempSubdirectory("sitebuilder-real-").FullName;
        try
        {
            var context = BuildPipeline.Default().Run(new BuildOptions(root, output, TestSite.Today));

            foreach (var draft in context.Content.Articles.Where(a => a.Visibility == Content.Visibility.Draft))
            {
                Assert.False(Directory.Exists(Path.Combine(output, "articles", draft.Id)));
            }

            Assert.True(File.Exists(Path.Combine(output, "medical", "index.html")));
        }
        finally
        {
            Directory.Delete(output, recursive: true);
        }
    }
}
