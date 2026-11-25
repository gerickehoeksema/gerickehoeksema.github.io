using System.Text.Encodings.Web;
using System.Text.Json;
using SiteBuilder.Content;
using static SiteBuilder.Rendering.Html;

namespace SiteBuilder.Rendering;

/// <summary>Templates for the profile header, page metadata and the About / Home sections.</summary>
internal static class ProfileSections
{
    private static readonly JsonSerializerOptions JsonLdOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Default, // escapes '<', so the JSON can't close the script tag
    };

    /// <summary>&lt;meta name="description"&gt; and &lt;title&gt;.</summary>
    public static string Meta(Profile profile) => $"""
        <meta name="description" content="{E(profile.Description)}">
        <title>{E(profile.Title)}</title>
        """;

    /// <summary>Open Graph title, description and profile names.</summary>
    public static string Social(Profile profile) => $"""
        <meta property="og:title" content="{E(profile.Title)}">
        <meta property="og:description" content="{E(profile.ShareDescription)}">
        <meta property="profile:first_name" content="{E(profile.GivenName)}">
        <meta property="profile:last_name" content="{E(profile.FamilyName)}">
        """;

    /// <summary>schema.org Person, for search engines.</summary>
    public static string JsonLd(Profile profile, string siteUrl)
    {
        var site = siteUrl.TrimEnd('/');
        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Person",
            ["name"] = profile.Name,
            ["url"] = site + "/",
            ["image"] = $"{site}/{profile.Image.TrimStart('/')}",
            ["jobTitle"] = profile.JobTitle,
            ["address"] = new Dictionary<string, string>
            {
                ["@type"] = "PostalAddress",
                ["addressLocality"] = profile.Location.Locality,
                ["addressCountry"] = profile.Location.CountryCode,
            },
            ["knowsAbout"] = profile.KnowsAbout,
            ["sameAs"] = profile.SameAs,
        }, JsonLdOptions);

        return $"""
            <script type="application/ld+json">
            {json}
            </script>
            """;
    }

    /// <summary>Name, headline and location in the profile header.</summary>
    public static string Header(Profile profile) => $"""
        <h1 class="profile-name">{E(profile.Name)}</h1>
        <p class="profile-headline">{E(profile.Headline)}</p>
        <p class="profile-location">{Icon("map-pin")} {E(profile.Location.Display)}</p>
        """;

    public static string LinkButtons(IEnumerable<ProfileLink> links) => string.Join("\n", links.Select(LinkButton));

    public static string LinkButton(ProfileLink link)
    {
        var cssClass = link.Primary ? "btn btn-primary"
            : link.Brand is null ? "btn btn-social"
            : $"btn btn-social brand-{link.Brand}";
        var newTab = Links.OpensInNewTab(link.Url);
        var target = newTab ? """ target="_blank" rel="noopener noreferrer" """.TrimEnd() : "";
        var note = newTab ? NewTabNote() : "";

        return $"""<a href="{E(Links.Href(link.Url))}"{target} class="{E(cssClass)}">{Icon(link.Icon)}{E(link.Label)}{note}</a>""";
    }

    public static string Skills(IEnumerable<Skill> skills) => string.Join("\n", skills.Select(skill => $"""
        <div class="skill-item">
            <div class="tile-icon">{Icon(skill.Icon)}</div>
            <div class="skill-info">
                <h3>{E(skill.Title)}</h3>
                <p>{E(skill.Description)}</p>
            </div>
        </div>
        """));

    public static string Competencies(IEnumerable<CompetencyGroup> groups) => string.Join("\n", groups.Select(group => $"""
        <div class="competency-item">
            <h4>{E(group.Title)}</h4>
            <ul>
        {string.Join("\n", group.Items.Select(item => $"        <li>{E(item)}</li>"))}
            </ul>
        </div>
        """));

    public static string Education(IEnumerable<EducationItem> items) => string.Join("\n", items.Select(item => $"""
        <div class="education-item">
            <h3>{E(item.School)}</h3>
            <p class="university-name">{E(item.Qualification)}</p>
            <p class="education-date">{E(item.Years)}</p>
        </div>
        """));

    public static string Copyright(Profile profile, DateOnly today) =>
        $"<p>&copy; {today.Year} {E(profile.Name)}. All rights reserved.</p>";
}
