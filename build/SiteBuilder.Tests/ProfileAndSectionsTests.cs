using SiteBuilder.Pipeline;

namespace SiteBuilder.Tests;

public class ProfileAndSectionsTests
{
    [Fact]
    public void The_job_title_drives_title_share_card_and_json_ld()
    {
        using var site = new TestSite().WithJson("profile.json", TestSite.DefaultProfile.Replace("Software Architect", "Tech Lead"));

        site.Build();

        var html = site.Read("index.html");
        Assert.Contains("<title>Test Person - Tech Lead</title>", html);
        Assert.Contains("<meta property=\"og:title\" content=\"Test Person - Tech Lead\">", html);
        Assert.Contains("\"jobTitle\": \"Tech Lead\"", html);
        Assert.DoesNotContain("Software Architect", html);
        Assert.DoesNotContain("fallback", html);
    }

    [Fact]
    public void Header_shows_name_headline_and_location()
    {
        using var site = new TestSite();

        site.Build();

        var html = site.Read("index.html");
        Assert.Contains("<h1 class=\"profile-name\">Test Person</h1>", html);
        Assert.Contains("<p class=\"profile-headline\">Builder of things</p>", html);
        Assert.Contains("#i-map-pin\"/></svg> Jeffreys Bay, South Africa</p>", html);
        Assert.Contains("<meta name=\"description\" content=\"Search description\">", html);
        Assert.Contains("<meta property=\"og:description\" content=\"Share description\">", html);
        Assert.Contains("\"image\": \"https://gerickehoeksema.co.za/images/profile-600.webp\"", html);
    }

    [Fact]
    public void Json_ld_cannot_break_out_of_its_script_tag()
    {
        using var site = new TestSite().WithJson("profile.json", TestSite.DefaultProfile.Replace("\"C#\"", "\"</script><script>alert(1)</script>\""));

        site.Build();

        Assert.DoesNotContain("</script><script>alert(1)", site.Read("index.html"));
    }

    [Fact]
    public void A_missing_profile_fails_with_a_clear_message()
    {
        using var site = new TestSite();
        File.Delete(Path.Combine(site.Root, "content", "profile.json"));

        var ex = Assert.Throws<BuildException>(() => site.Build());

        Assert.Contains("profile.json", ex.Message);
    }

    [Fact]
    public void Profile_is_strict_about_field_names()
    {
        using var site = new TestSite().WithJson("profile.json", TestSite.DefaultProfile.Replace("\"jobTitle\"", "\"jobtitle\""));

        Assert.Throws<BuildException>(() => site.Build());
    }

    [Fact]
    public void Links_render_primary_brand_and_placeholder_buttons()
    {
        using var site = new TestSite().WithJson("links.json", """
            [
              { "id": "email", "label": "Email", "url": "https://mail.example.com/?a=1&b=2", "icon": "mail", "primary": true },
              { "id": "linkedin", "label": "LinkedIn", "url": "https://www.linkedin.com/in/x/", "icon": "linkedin", "brand": "linkedin" },
              { "id": "goodreads", "label": "Goodreads", "url": "#", "icon": "goodreads", "brand": "goodreads" },
              { "id": "secret", "label": "Secret", "url": "https://example.com", "icon": "link", "visibility": "draft" }
            ]
            """);

        site.Build();

        var html = site.Read("index.html");
        Assert.Contains("<a href=\"https://mail.example.com/?a=1&amp;b=2\" target=\"_blank\" rel=\"noopener noreferrer\" class=\"btn btn-primary\">", html);
        Assert.Contains("class=\"btn btn-social brand-linkedin\"><svg class=\"icon\" aria-hidden=\"true\"><use href=\"#i-linkedin\"/></svg>LinkedIn<span class=\"visually-hidden\"> (opens in a new tab)</span></a>", html);
        Assert.Contains("<a href=\"#\" class=\"btn btn-social brand-goodreads\"><svg class=\"icon\" aria-hidden=\"true\"><use href=\"#i-goodreads\"/></svg>Goodreads</a>", html);
        Assert.DoesNotContain("Secret", html);
    }

    [Fact]
    public void Intro_and_about_are_rendered_from_markdown()
    {
        using var site = new TestSite()
            .WithText("intro.md", "First paragraph with a [link](https://example.com).\n\nSecond paragraph.\n")
            .WithText("about.md", "With **15+ years** of experience.\n");

        site.Build();

        var html = site.Read("index.html");
        Assert.Contains("<p>First paragraph with a <a href=\"https://example.com\">link</a>.</p>", html);
        Assert.Contains("<p>Second paragraph.</p>", html);
        Assert.Contains("<p>With <strong>15+ years</strong> of experience.</p>", html);
    }

    [Fact]
    public void Section_text_with_front_matter_is_rejected()
    {
        using var site = new TestSite().WithText("intro.md", "---\ntitle: nope\n---\nText");

        var ex = Assert.Throws<BuildException>(() => site.Build());

        Assert.Contains("intro.md", ex.Message);
    }

    [Fact]
    public void Skills_competencies_and_education_render_and_respect_visibility()
    {
        using var site = new TestSite()
            .WithJson("skills.json", """
                [
                  { "id": "csharp", "title": "C# & .NET", "description": "Enterprise apps", "icon": "code" },
                  { "id": "hidden", "title": "Hidden skill", "description": "Nope", "icon": "code", "visibility": "draft" }
                ]
                """)
            .WithJson("competencies.json", """
                [{ "id": "arch", "title": "Architecture", "items": ["Monolith decomposition", "API design"] }]
                """)
            .WithJson("education.json", """
                [
                  { "id": "cti", "school": "CTI", "qualification": "Software Development", "start": 2008, "end": 2009 },
                  { "id": "school", "school": "High School", "qualification": "Matric", "end": 2007 }
                ]
                """);

        site.Build();

        var html = site.Read("index.html");
        Assert.Contains("<h3>C# &amp; .NET</h3>", html);
        Assert.Contains("#i-code", html);
        Assert.DoesNotContain("Hidden skill", html);
        Assert.Contains("<h4>Architecture</h4>", html);
        Assert.Contains("<li>Monolith decomposition</li>", html);
        Assert.Contains("<p class=\"education-date\">2008 – 2009</p>", html);
        Assert.Contains("<p class=\"education-date\">2007</p>", html);
    }

    [Fact]
    public void Copyright_year_follows_the_build_date()
    {
        using var site = new TestSite();

        site.Build(today: new DateOnly(2027, 1, 2));

        Assert.Contains("<p>&copy; 2027 Test Person. All rights reserved.</p>", site.Read("index.html"));
    }
}
