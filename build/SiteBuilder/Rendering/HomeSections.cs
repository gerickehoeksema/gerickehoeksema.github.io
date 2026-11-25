using SiteBuilder.Content;
using static SiteBuilder.Rendering.Html;

namespace SiteBuilder.Rendering;

/// <summary>Templates for the content-driven parts of index.html. One template per content type.</summary>
internal static class HomeSections
{
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

    public static string Certifications(IEnumerable<Certification> items) =>
        string.Join("\n", items.Select(Certification));

    public static string Certification(Certification item) => $"""
        <a class="cert-item" href="{E(Links.Href(item.Link))}" target="_blank" rel="noopener noreferrer">
            <div class="tile-icon">{Icon("award")}</div>
            <div class="cert-info">
                <h3>{E(item.Title)}</h3>
                <p>{E(item.Issuer)} · {E(item.LinkText)}{NewTabNote(Links.Kind(item.Link))}</p>
            </div>
        </a>
        """;

    public static string Projects(IEnumerable<Project> items) =>
        string.Join("\n", items.Select(Project));

    public static string Project(Project item)
    {
        var iconClass = item.Brand is null ? "tile-icon" : $"tile-icon brand-{item.Brand}";

        return $"""
            <a class="project-card" href="{E(Links.Href(item.Url))}" target="_blank" rel="noopener noreferrer">
                <div class="{E(iconClass)}">{Icon(item.Icon)}</div>
                <div class="project-info">
                    <h3>{E(item.Title)}</h3>
                    <p>{E(item.Description)}</p>
                    <span class="project-link">{E(item.LinkText)} {Icon("arrow-up-right")}{NewTabNote()}</span>
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
}
