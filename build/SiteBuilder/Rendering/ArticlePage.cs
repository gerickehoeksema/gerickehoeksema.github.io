using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using SiteBuilder.Content;
using SiteBuilder.Pipeline;
using static SiteBuilder.Rendering.Html;

namespace SiteBuilder.Rendering;

/// <summary>The full HTML page for one article.</summary>
internal static class ArticlePage
{
    private static readonly JsonSerializerOptions JsonLdOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Default, // escapes '<', so the JSON can't close the script tag
    };

    public static string Render(Article article, BuildOptions options)
    {
        var site = options.SiteUrl.TrimEnd('/');
        var url = site + article.Path;
        var image = File.Exists(Path.Combine(article.SourceDirectory, "og.jpg"))
            ? url + "og.jpg"
            : site + "/images/og-image.jpg";
        var date = article.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var displayDate = article.Date.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);

        var notPublic = NotPublicReason(article, options.Today);
        var robots = notPublic is null ? "" : "\n    <meta name=\"robots\" content=\"noindex\">";
        var banner = notPublic is null
            ? ""
            : $"""

                          <p class="draft-banner" role="note"><strong>{E(notPublic)}.</strong> This preview is not on the live site. Highlighted text marks details still to fill in.</p>
              """;

        var jsonLd = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BlogPosting",
            ["headline"] = article.Title,
            ["description"] = article.Description,
            ["url"] = url,
            ["image"] = image,
            ["datePublished"] = date,
            ["author"] = new Dictionary<string, string> { ["@type"] = "Person", ["name"] = "Gericke Hoeksema", ["url"] = site + "/" },
            ["keywords"] = article.Keywords,
        }, JsonLdOptions);

        var kicker = article.Kicker is null ? "" : $"""<p class="article-kicker">{E(article.Kicker)}</p>""";
        var shareUrl = "https://www.linkedin.com/sharing/share-offsite/?url=" + Uri.EscapeDataString(url);

        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">{{robots}}
                <title>{{E(article.Title)}} · Gericke Hoeksema</title>
                <meta name="description" content="{{E(article.Description)}}">
                <meta name="author" content="Gericke Hoeksema">
                <link rel="canonical" href="{{E(url)}}">
                <link rel="icon" type="image/svg+xml" href="/favicon.svg">
                <link rel="preconnect" href="https://fonts.googleapis.com">
                <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
                <link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&amp;family=Space+Grotesk:wght@500;600;700&amp;display=swap">
                <link rel="stylesheet" href="/styles.css">
                <script>
                    try {
                        const theme = localStorage.getItem('theme');
                        if (theme === 'light' || theme === 'dark') document.documentElement.dataset.theme = theme;
                    } catch (e) { /* storage blocked: follow the OS preference */ }
                </script>

                <!-- Social sharing -->
                <meta property="og:type" content="article">
                <meta property="og:site_name" content="Gericke Hoeksema">
                <meta property="og:title" content="{{E(article.Title)}}">
                <meta property="og:description" content="{{E(article.Description)}}">
                <meta property="og:url" content="{{E(url)}}">
                <meta property="og:image" content="{{E(image)}}">
                <meta property="og:image:width" content="1200">
                <meta property="og:image:height" content="630">
                <meta property="article:published_time" content="{{date}}">
                <meta name="twitter:card" content="summary_large_image">
                <meta name="theme-color" content="#0c1624" media="(prefers-color-scheme: dark)">
                <meta name="theme-color" content="#f7f1e6" media="(prefers-color-scheme: light)">

                <script type="application/ld+json">
            {{jsonLd}}
                </script>
            </head>
            <body>
            {{Sprite}}

                <a href="#main" class="skip-link">Skip to content</a>
                <nav class="navbar" aria-label="Main">
                    <div class="nav-container">
                        <a href="/" class="nav-brand">Gericke Hoeksema</a>
                        <ul class="nav-menu">
                            <li><a href="/#home" class="nav-link">Home</a></li>
                            <li><a href="/#about" class="nav-link">About Me</a></li>
                            <li><a href="/#experience" class="nav-link">Experience</a></li>
                            <li><a href="/#projects" class="nav-link">Projects</a></li>
                            <li><a href="/#blog" class="nav-link active" aria-current="page">Articles</a></li>
                        </ul>
                        <button type="button" class="theme-toggle" id="theme-toggle" aria-label="Toggle light and dark theme">
                            {{Icon("sun", "icon-sun")}}
                            {{Icon("moon", "icon-moon")}}
                        </button>
                    </div>
                </nav>

                <main id="main" class="main-content" tabindex="-1">
                    <article class="article">{{banner}}
                        <a href="/#blog" class="article-back">{{Icon("arrow-left")}} All articles</a>

                        <header class="article-header">
                            {{kicker}}
                            <h1>{{E(article.Title)}}</h1>
                            <p class="article-lede">{{E(article.Lede ?? article.Description)}}</p>
                            <p class="article-byline">
                                <span>Gericke Hoeksema</span>
                                <span aria-hidden="true">·</span>
                                <time datetime="{{date}}">{{displayDate}}</time>
                                <span aria-hidden="true">·</span>
                                <span class="article-reading">{{Icon("clock")}} {{article.ReadingMinutes}} min read</span>
                            </p>
                        </header>

                        <div class="prose">
            {{article.BodyHtml}}
                        </div>

                        <footer class="article-footer">
                            <p class="article-share-label">Share this article</p>
                            <div class="article-share">
                                <a class="btn btn-social brand-linkedin" href="{{E(shareUrl)}}" target="_blank" rel="noopener noreferrer">{{Icon("linkedin")}}Share on LinkedIn</a>
                                <button type="button" class="btn" data-copy-link>{{Icon("link")}}<span>Copy link</span></button>
                            </div>
                        </footer>
                    </article>
                </main>

                <footer class="footer">
                    <div class="footer-content">
                        <p>&copy; {{options.Today.Year}} Gericke Hoeksema. All rights reserved.</p>
                        <div class="footer-links">
                            <a href="/#home">Home</a>
                            <a href="/#about">About Me</a>
                            <a href="/#blog">Articles</a>
                        </div>
                    </div>
                </footer>

                <script src="/script.js"></script>
            </body>
            </html>

            """;
    }

    /// <summary>Null when the article is live; otherwise why it is only visible in a preview build.</summary>
    internal static string? NotPublicReason(Article article, DateOnly today) => article switch
    {
        { Visibility: Visibility.Draft } => "Draft",
        { PublishFrom: { } from } when from > today => $"Scheduled for {from.ToString("d MMMM yyyy", CultureInfo.InvariantCulture)}",
        _ => null,
    };

    /// <summary>Icons used on article pages. UI icons: Lucide (ISC). Brand icons: Simple Icons (CC0).</summary>
    private const string Sprite = """
            <svg xmlns="http://www.w3.org/2000/svg" hidden>
                <defs>
                    <symbol id="i-linkedin" viewBox="0 0 24 24" fill="currentColor"><path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/></symbol>
                    <symbol id="i-sun" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="4"/><path d="M12 2v2"/><path d="M12 20v2"/><path d="m4.93 4.93 1.41 1.41"/><path d="m17.66 17.66 1.41 1.41"/><path d="M2 12h2"/><path d="M20 12h2"/><path d="m6.34 17.66-1.41 1.41"/><path d="m19.07 4.93-1.41 1.41"/></symbol>
                    <symbol id="i-moon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z"/></symbol>
                    <symbol id="i-arrow-left" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m12 19-7-7 7-7"/><path d="M19 12H5"/></symbol>
                    <symbol id="i-link" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/></symbol>
                    <symbol id="i-clock" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></symbol>
                </defs>
            </svg>
        """;
}
