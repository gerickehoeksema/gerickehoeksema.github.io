using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SiteBuilder.Content;

/// <summary>Thrown when a content file is missing a field or can't be parsed. The message names the file.</summary>
public sealed class ContentException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>
/// Reads <c>content/</c> into typed records:
/// <list type="bullet">
/// <item><c>articles/&lt;id&gt;/index.md</c>: YAML front matter + Markdown body.</item>
/// <item><c>experience.json</c>: an array of roles.</item>
/// </list>
/// Parsing is strict: an unknown field is an error, so a typo like "visiblity" fails the build
/// instead of silently publishing a draft.
/// </summary>
public sealed class ContentLoader
{
    private static readonly MarkdownPipeline Markdown = new MarkdownPipelineBuilder()
        .UseYamlFrontMatter()
        .UseAutoIdentifiers(Markdig.Extensions.AutoIdentifiers.AutoIdentifierOptions.GitHub)
        .UseGenericAttributes() // must be last: allows {.class} on blocks
        .Build();

    private static readonly IDeserializer Yaml = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false),
            new YearMonthJsonConverter(),
        },
    };

    public SiteContent Load(string contentDirectory)
    {
        if (!Directory.Exists(contentDirectory))
        {
            throw new ContentException($"Content folder not found: {contentDirectory}");
        }

        return new SiteContent(
            LoadArticles(Path.Combine(contentDirectory, "articles")),
            LoadExperience(Path.Combine(contentDirectory, "experience.json")));
    }

    private static IReadOnlyList<Article> LoadArticles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory.EnumerateDirectories(directory)
            .Select(dir => Path.Combine(dir, "index.md"))
            .Where(File.Exists)
            .Select(ParseArticle)
            .OrderByDescending(a => a.Date)
            .ToList();
    }

    internal static Article ParseArticle(string path)
    {
        var markdown = File.ReadAllText(path);
        var document = Markdig.Markdown.Parse(markdown, Markdown);

        var frontMatterBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault()
            ?? throw new ContentException($"{path}: missing YAML front matter (--- ... ---) at the top.");

        ArticleFrontMatter fm;
        try
        {
            fm = Yaml.Deserialize<ArticleFrontMatter>(frontMatterBlock.Lines.ToString())
                ?? throw new ContentException($"{path}: front matter is empty.");
        }
        catch (YamlException ex)
        {
            throw new ContentException($"{path}: invalid front matter. {ex.Message}", ex);
        }

        var id = Path.GetFileName(Path.GetDirectoryName(path))!;
        if (fm.Id is not null && fm.Id != id)
        {
            throw new ContentException($"{path}: front matter id \"{fm.Id}\" must match the folder name \"{id}\".");
        }

        return new Article
        {
            Id = id,
            Title = Required(fm.Title, nameof(fm.Title), path),
            Description = Required(fm.Description, nameof(fm.Description), path),
            Lede = fm.Lede,
            Kicker = fm.Kicker,
            Icon = fm.Icon ?? "pen-line",
            Date = ParseDate(Required(fm.Date, nameof(fm.Date), path), path),
            Keywords = fm.Keywords ?? [],
            Lenses = fm.Lens?.Select(l => ParseEnum<Lens>(l, path)).ToList() ?? [Lens.Work],
            Visibility = fm.Visibility is null ? Visibility.Public : ParseEnum<Visibility>(fm.Visibility, path),
            PublishFrom = fm.PublishFrom is null ? null : ParseDate(fm.PublishFrom, path),
            Featured = fm.Featured ?? false,
            BodyHtml = document.ToHtml(Markdown),
            WordCount = CountWords(Markdig.Markdown.ToPlainText(markdown, Markdown)),
            SourceDirectory = Path.GetDirectoryName(path)!,
        };
    }

    private static IReadOnlyList<ExperienceItem> LoadExperience(string path)
    {
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            var items = JsonSerializer.Deserialize<List<ExperienceItem>>(File.ReadAllText(path), Json)
                ?? throw new ContentException($"{path}: expected a JSON array of roles.");

            return items.OrderByDescending(e => e.Start).ToList();
        }
        catch (JsonException ex)
        {
            throw new ContentException($"{path}: {ex.Message}", ex);
        }
    }

    private static string Required(string? value, string field, string path) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ContentException($"{path}: front matter is missing \"{char.ToLowerInvariant(field[0])}{field[1..]}\".")
            : value;

    private static DateOnly ParseDate(string value, string path) =>
        DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : throw new ContentException($"{path}: \"{value}\" is not a date in yyyy-MM-dd format.");

    private static T ParseEnum<T>(string value, string path) where T : struct, Enum =>
        Enum.TryParse<T>(value, ignoreCase: true, out var result) && Enum.IsDefined(result)
            ? result
            : throw new ContentException($"{path}: \"{value}\" is not a valid {typeof(T).Name.ToLowerInvariant()} ({string.Join(", ", Enum.GetNames<T>()).ToLowerInvariant()}).");

    private static int CountWords(string text) =>
        text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    /// <summary>Front matter as written. Strings only, so errors can be reported with the file name.</summary>
    private sealed class ArticleFrontMatter
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Lede { get; set; }
        public string? Kicker { get; set; }
        public string? Icon { get; set; }
        public string? Date { get; set; }
        public List<string>? Keywords { get; set; }
        public List<string>? Lens { get; set; }
        public string? Visibility { get; set; }
        public string? PublishFrom { get; set; }
        public bool? Featured { get; set; }
    }

    private sealed class YearMonthJsonConverter : JsonConverter<YearMonth>
    {
        public override YearMonth Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                return YearMonth.Parse(reader.GetString() ?? string.Empty);
            }
            catch (FormatException ex)
            {
                throw new JsonException(ex.Message, ex);
            }
        }

        public override void Write(Utf8JsonWriter writer, YearMonth value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
    }
}
