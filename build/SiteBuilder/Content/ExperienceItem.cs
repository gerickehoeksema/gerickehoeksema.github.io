namespace SiteBuilder.Content;

/// <summary>One role on the Experience timeline, from <c>content/experience.json</c>.</summary>
public sealed record ExperienceItem : ContentItem
{
    public required string Role { get; init; }

    public required string Company { get; init; }

    public required YearMonth Start { get; init; }

    /// <summary>Null while the role is current.</summary>
    public YearMonth? End { get; init; }

    public required string Summary { get; init; }

    public IReadOnlyList<string> Highlights { get; init; } = [];
}

/// <summary>A month precision date, written as "yyyy-MM" in content files.</summary>
public readonly record struct YearMonth(int Year, int Month) : IComparable<YearMonth>
{
    public static YearMonth Parse(string value)
    {
        var parts = value.Split('-');
        if (parts.Length != 2
            || !int.TryParse(parts[0], out var year)
            || !int.TryParse(parts[1], out var month)
            || month is < 1 or > 12)
        {
            throw new FormatException($"Expected a year and month as \"yyyy-MM\", got \"{value}\".");
        }

        return new YearMonth(year, month);
    }

    public int CompareTo(YearMonth other) => (Year, Month).CompareTo((other.Year, other.Month));

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
