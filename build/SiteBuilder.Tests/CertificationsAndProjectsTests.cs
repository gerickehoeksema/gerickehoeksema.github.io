using SiteBuilder.Content;
using SiteBuilder.Pipeline;
using SiteBuilder.Rendering;

namespace SiteBuilder.Tests;

public class CertificationsAndProjectsTests
{
    [Fact]
    public void Certifications_render_in_file_order_with_encoded_links()
    {
        using var site = new TestSite()
            .WithFile("certifications/Programming in C#.pdf")
            .WithJson("certifications.json", """
                [
                  { "id": "csharp", "title": "Programming in C#", "issuer": "Microsoft", "link": "certifications/Programming in C#.pdf" },
                  { "id": "online", "title": "Verified online", "issuer": "Acme", "link": "https://example.com/verify/1", "linkText": "Verify" }
                ]
                """);

        site.Build();

        var html = site.Read("index.html");
        Assert.Contains("href=\"certifications/Programming%20in%20C%23.pdf\"", html);
        Assert.Contains("<h3>Programming in C#</h3>", html);
        Assert.Contains("<p>Microsoft · View certificate<span class=\"visually-hidden\"> (PDF, opens in a new tab)</span></p>", html);
        Assert.Contains("href=\"https://example.com/verify/1\"", html);
        Assert.Contains("<p>Acme · Verify<span class=\"visually-hidden\"> (opens in a new tab)</span></p>", html);
        Assert.True(html.IndexOf("Programming in C#", StringComparison.Ordinal) < html.IndexOf("Verified online", StringComparison.Ordinal));
        Assert.DoesNotContain("fallback certifications", html);
    }

    [Fact]
    public void A_missing_certificate_file_fails_the_build()
    {
        using var site = new TestSite().WithJson("certifications.json", """
            [{ "id": "gone", "title": "Gone", "issuer": "Microsoft", "link": "certifications/Renamed.pdf" }]
            """);

        var ex = Assert.Throws<BuildException>(() => site.Build());

        Assert.Contains("certifications/Renamed.pdf", ex.Message);
    }

    [Fact]
    public void A_hidden_certificate_with_a_missing_file_does_not_fail_the_build()
    {
        using var site = new TestSite().WithJson("certifications.json", """
            [{ "id": "wip", "title": "Not yet", "issuer": "Microsoft", "link": "certifications/Later.pdf", "visibility": "draft" }]
            """);

        site.Build();

        Assert.DoesNotContain("Not yet", site.Read("index.html"));
    }

    [Fact]
    public void Projects_render_with_icon_brand_and_link_text()
    {
        using var site = new TestSite().WithJson("projects.json", """
            [
              { "id": "gh", "title": "GitHub", "description": "Repos.", "url": "https://github.com/x", "linkText": "View my repositories", "icon": "github", "brand": "github" },
              { "id": "secret", "title": "Secret project", "description": "Shh.", "url": "https://example.com", "visibility": "draft" }
            ]
            """);

        site.Build();

        var html = site.Read("index.html");
        Assert.Contains("<div class=\"tile-icon brand-github\"><svg class=\"icon\" aria-hidden=\"true\"><use href=\"#i-github\"/></svg></div>", html);
        Assert.Contains("View my repositories", html);
        Assert.DoesNotContain("Secret project", site.AllOutputText());
    }

    [Fact]
    public void Project_defaults_are_applied()
    {
        var html = HomeSections.Project(new Project { Id = "p", Title = "T", Description = "D", Url = "https://example.com" });

        Assert.Contains("<div class=\"tile-icon\">", html);
        Assert.Contains("#i-code", html);
        Assert.Contains("Visit project", html);
        Assert.Contains("<span class=\"visually-hidden\"> (opens in a new tab)</span>", html);
    }

    [Fact]
    public void Duplicate_ids_are_rejected()
    {
        using var site = new TestSite().WithJson("projects.json", """
            [
              { "id": "same", "title": "A", "description": "D", "url": "https://a.example" },
              { "id": "same", "title": "B", "description": "D", "url": "https://b.example" }
            ]
            """);

        var ex = Assert.Throws<ContentException>(() => new ContentLoader().Load(Path.Combine(site.Root, "content")));

        Assert.Contains("\"same\"", ex.Message);
    }

    [Theory]
    [InlineData("certifications/Programming in C#.pdf", "certifications/Programming%20in%20C%23.pdf")]
    [InlineData("https://example.com/a b", "https://example.com/a b")]
    [InlineData("mailto:me@example.com", "mailto:me@example.com")]
    public void Links_encode_local_paths_only(string link, string expected) =>
        Assert.Equal(expected, Links.Href(link));

    [Theory]
    [InlineData("certifications/Programming in C#.pdf", "PDF")]
    [InlineData("https://example.com/cert.PDF?download=1", "PDF")]
    [InlineData("https://example.com/verify/1", null)]
    public void Links_say_when_they_point_at_a_pdf(string link, string? expected) =>
        Assert.Equal(expected, Links.Kind(link));
}
