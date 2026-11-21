using SiteBuilder.Content;
using SiteBuilder.Specifications;

namespace SiteBuilder.Tests;

public class VisibilityTests
{
    private static readonly DateOnly Today = new(2026, 11, 21);

    private static ExperienceItem Item(Visibility visibility = Visibility.Public, DateOnly? publishFrom = null, params Lens[] lenses) => new()
    {
        Id = "x",
        Role = "Role",
        Company = "Company",
        Start = new YearMonth(2020, 1),
        Summary = "Summary",
        Visibility = visibility,
        PublishFrom = publishFrom,
        Lenses = lenses.Length == 0 ? [Lens.Work] : lenses,
    };

    [Theory]
    [InlineData(Visibility.Public, true)]
    [InlineData(Visibility.Unlisted, true)]
    [InlineData(Visibility.Draft, false)]
    public void NotDraft_excludes_only_drafts(Visibility visibility, bool expected) =>
        Assert.Equal(expected, new NotDraft().IsSatisfiedBy(Item(visibility)));

    [Fact]
    public void PublishedBy_hides_scheduled_items_until_their_date()
    {
        var spec = new PublishedBy(Today);

        Assert.True(spec.IsSatisfiedBy(Item()));
        Assert.True(spec.IsSatisfiedBy(Item(publishFrom: Today)));
        Assert.False(spec.IsSatisfiedBy(Item(publishFrom: Today.AddDays(1))));
    }

    [Fact]
    public void InLens_matches_items_in_that_lens()
    {
        Assert.True(new InLens(Lens.Life).IsSatisfiedBy(Item(lenses: [Lens.Work, Lens.Life])));
        Assert.False(new InLens(Lens.Life).IsSatisfiedBy(Item()));
    }

    [Fact]
    public void Composition_combines_rules()
    {
        var liveWork = new NotDraft().And(new PublishedBy(Today)).And(new InLens(Lens.Work));

        Assert.True(liveWork.IsSatisfiedBy(Item()));
        Assert.False(liveWork.IsSatisfiedBy(Item(Visibility.Draft)));
        Assert.False(liveWork.IsSatisfiedBy(Item(lenses: Lens.Life)));
        Assert.True(new NotDraft().Not().IsSatisfiedBy(Item(Visibility.Draft)));
        Assert.True(new NotDraft().Or(new Unlisted()).IsSatisfiedBy(Item(Visibility.Unlisted)));
    }

    [Theory]
    //          visibility           scheduled preview  rendered listed
    [InlineData(Visibility.Public,   false,    false,   true,    true)]
    [InlineData(Visibility.Unlisted, false,    false,   true,    false)]
    [InlineData(Visibility.Draft,    false,    false,   false,   false)]
    [InlineData(Visibility.Public,   true,     false,   false,   false)]
    [InlineData(Visibility.Draft,    false,    true,    true,    true)]
    [InlineData(Visibility.Public,   true,     true,    true,    true)]
    [InlineData(Visibility.Unlisted, false,    true,    true,    false)]
    public void Policy_decides_what_is_rendered_and_listed(Visibility visibility, bool scheduled, bool preview, bool rendered, bool listed)
    {
        var policy = new VisibilityPolicy(Today, preview);
        var item = Item(visibility, scheduled ? Today.AddDays(10) : null);

        Assert.Equal(rendered, policy.Rendered.IsSatisfiedBy(item));
        Assert.Equal(listed, policy.Listed.IsSatisfiedBy(item));
    }
}
