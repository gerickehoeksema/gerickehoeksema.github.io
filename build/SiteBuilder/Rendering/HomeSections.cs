using System.Globalization;
using SiteBuilder.Content;
using static SiteBuilder.Rendering.Html;

namespace SiteBuilder.Rendering;

/// <summary>Templates for the content-driven parts of index.html. One template per content type.</summary>
internal static class HomeSections
{
    public static string Experience(IEnumerable<ExperienceItem> items) =>
        string.Join("\n\n", items.Select(ExperienceEntry));

    public static string ExperienceEntry(ExperienceItem item)
    {
        // Interpolated values aren't re-indented by raw strings, so this block carries its own indentation.
        var highlights = item.Highlights.Count == 0
            ? ""
            : "\n        <ul class=\"experience-achievements\">\n"
              + string.Join("\n", item.Highlights.Select(h => $"            <li>{E(h)}</li>"))
              + "\n        </ul>";

        return $"""
            <div class="timeline-item">
                <div class="timeline-marker"></div>
                <div class="timeline-content">
                    <div class="experience-header">
                        <div>
                            <h3>{E(item.Role)}</h3>
                            <p class="company-name">{E(item.Company)}</p>
                        </div>
                        <div class="experience-date">{E(DateRange(item))}</div>
                    </div>
                    <p class="experience-description">{E(item.Summary)}</p>{highlights}
                </div>
            </div>
            """;
    }

    public static string DateRange(ExperienceItem item) =>
        $"{Month(item.Start)} – {(item.End is { } end ? Month(end) : "Present")}";

    public static string ArticleCards(IReadOnlyList<Article> articles) =>
        articles.Count == 0 ? EmptyArticles : string.Join("\n", articles.Select(ArticleCard));

    public static string ArticleCard(Article article)
    {
        var meta = string.Join(" · ", new[]
        {
            article.Visibility == Visibility.Draft ? "Draft" : null,
            article.Kicker,
            $"{article.ReadingMinutes} min read",
        }.Where(s => s is not null));

        return $"""
            <a class="article-card" href="{E(article.Path.TrimStart('/'))}">
                <div class="article-image-placeholder">
                    {Icon(article.Icon)}
                </div>
                <div class="article-content">
                    <h3>{E(article.Title)}</h3>
                    <p class="article-meta">{E(meta)}</p>
                    <p>{E(article.Description)}</p>
                </div>
            </a>
            """;
    }

    /// <summary>Shown while nothing is published yet. Keep in sync with the fallback in index.html.</summary>
    public const string EmptyArticles = """
        <div class="article-card">
            <div class="article-image-placeholder">
                <svg class="icon" aria-hidden="true"><use href="#i-pen-line"/></svg>
            </div>
            <div class="article-content">
                <h3>First article on the way</h3>
                <p class="article-meta">Architecture · Azure · .NET</p>
                <p>Lessons from real projects: service boundaries, cloud architecture and the decisions behind them.</p>
            </div>
        </div>
        """;

    private static string Month(YearMonth value) =>
        new DateOnly(value.Year, value.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture);
}
