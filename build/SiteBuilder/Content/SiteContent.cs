namespace SiteBuilder.Content;

/// <summary>Everything loaded from <c>content/</c>, before any visibility rules are applied.</summary>
public sealed record SiteContent(
    IReadOnlyList<Article> Articles,
    IReadOnlyList<ExperienceItem> Experience);
