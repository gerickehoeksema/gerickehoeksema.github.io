using System.Text;
using System.Text.RegularExpressions;

namespace SiteBuilder.Rendering;

internal static class Html
{
    /// <summary>
    /// Encode text for HTML content or a quoted attribute. Pages are UTF-8, so only the five
    /// special characters are escaped (WebUtility.HtmlEncode would also turn "·" into "&amp;#183;").
    /// </summary>
    public static string E(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.AsSpan().IndexOfAny("&<>\"'") < 0)
        {
            return value ?? string.Empty;
        }

        var sb = new StringBuilder(value.Length + 16);
        foreach (var c in value)
        {
            sb.Append(c switch
            {
                '&' => "&amp;",
                '<' => "&lt;",
                '>' => "&gt;",
                '"' => "&quot;",
                '\'' => "&#39;",
                _ => c.ToString(),
            });
        }

        return sb.ToString();
    }

    public static string Icon(string id, string? extraClass = null) =>
        $"""<svg class="icon{(extraClass is null ? "" : " " + extraClass)}" aria-hidden="true"><use href="#i-{E(id)}"/></svg>""";

    /// <summary>Prefix every non-empty line with <paramref name="indent"/>.</summary>
    public static string Indent(string text, string indent) =>
        string.Join('\n', text.ReplaceLineEndings("\n").Split('\n').Select(l => l.Length == 0 ? l : indent + l));
}

/// <summary>
/// Replaces the content between <c>&lt;!-- build:name --&gt;</c> and <c>&lt;!-- /build:name --&gt;</c>.
/// The markers stay, so the hand-written page remains a valid fallback and can be rebuilt.
/// </summary>
internal static partial class Regions
{
    public static string Replace(string html, string name, string content)
    {
        // The opening marker may carry a note: <!-- build:name · generated from ... -->
        var open = StartMarker(name).Match(html);
        var end = $"<!-- /build:{name} -->";
        var endIndex = html.IndexOf(end, StringComparison.Ordinal);
        if (!open.Success || endIndex < open.Index)
        {
            throw new SiteBuilder.Pipeline.BuildException($"Region \"{name}\" not found: expected <!-- build:{name} --> ... {end}.");
        }

        // Indent the new content like the closing marker.
        var lineStart = html.LastIndexOf('\n', endIndex) + 1;
        var indent = LeadingWhitespace().Match(html[lineStart..endIndex]).Value;

        var inner = "\n" + Html.Indent(content.Trim('\n'), indent) + "\n" + indent;
        return html[..(open.Index + open.Length)] + inner + html[endIndex..];
    }

    [GeneratedRegex(@"^[ \t]*")]
    private static partial Regex LeadingWhitespace();

    private static Regex StartMarker(string name) =>
        new($@"<!-- build:{Regex.Escape(name)}(?:\s[^>]*)?-->", RegexOptions.CultureInvariant);
}

/// <summary>Links in content are written as plain paths; these turn them into safe hrefs.</summary>
internal static class Links
{
    /// <summary>True for a path inside the site, false for an absolute URL (https://, mailto:).</summary>
    public static bool IsLocal(string link) => !Uri.TryCreate(link, UriKind.Absolute, out _);

    /// <summary>Encodes each segment of a local path ("C#" becomes "C%23"); absolute URLs are left alone.</summary>
    public static string Href(string link) =>
        IsLocal(link)
            ? string.Join('/', link.Split('/').Select(Uri.EscapeDataString))
            : link;
}
