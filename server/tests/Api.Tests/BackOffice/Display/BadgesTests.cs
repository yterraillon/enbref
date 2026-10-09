using EnBref.Api.BackOffice.Display;
using EnBref.Api.Features.CollectHeadlines;
using EnBref.Api.Features.ReadPublishedRecap;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Collection;
using EnBref.Infrastructure.Llm;

namespace EnBref.Api.Tests.BackOffice.Display;

public class BadgesTests
{
    private static readonly Recap Recap = new(new DateOnly(2026, 10, 9), new Dictionary<Category, IReadOnlyList<Brief>>());

    [Test]
    public async Task Readable_artifact_is_available()
    {
        var badge = Badges.ForArtifact(RecapType.Daily, new ReadPublishedRecapResult("latest.json", Recap, "{}", Error: null));

        await Assert.That(badge).IsEqualTo(new Badge("Disponible", BadgeTone.Success));
    }

    [Test]
    [Arguments(RecapType.Daily, BadgeTone.Warning)]
    [Arguments(RecapType.Demo, BadgeTone.Warning)]
    [Arguments(RecapType.Test, BadgeTone.Neutral)]
    public async Task Missing_artifact_is_absent_and_only_a_warning_when_it_matters(RecapType type, BadgeTone tone)
    {
        var badge = Badges.ForArtifact(type, new ReadPublishedRecapResult(type.Artifact(), null, null, "Artefact absent.", IsMissing: true));

        await Assert.That(badge).IsEqualTo(new Badge("Absent", tone));
    }

    [Test]
    public async Task Artifact_that_cannot_be_read_is_unreadable()
    {
        var badge = Badges.ForArtifact(RecapType.Demo, new ReadPublishedRecapResult("demo.json", null, "{}", "Hors contrat."));

        await Assert.That(badge).IsEqualTo(new Badge("Illisible", BadgeTone.Danger));
    }

    [Test]
    [Arguments(2, 2, "2 sur 2 flux disponibles", BadgeTone.Success)]
    [Arguments(1, 2, "1 sur 2 flux disponibles", BadgeTone.Warning)]
    [Arguments(0, 2, "0 sur 2 flux disponibles", BadgeTone.Danger)]
    public async Task Collection_badge_counts_available_feeds(int available, int total, string label, BadgeTone tone)
    {
        var feeds = Enumerable.Range(0, total)
            .Select(index => new FeedResult(
                new Feed($"Source {index}", new Uri($"https://flux.test/{index}")),
                index < available ? FeedStatus.Available : FeedStatus.Unreachable,
                index < available ? ["Un titre"] : []))
            .ToList();

        await Assert.That(Badges.ForCollection(new CollectionResult(feeds))).IsEqualTo(new Badge(label, tone));
    }

    [Test]
    [Arguments(FeedStatus.Available, BadgeTone.Success)]
    [Arguments(FeedStatus.Empty, BadgeTone.Warning)]
    [Arguments(FeedStatus.Unreachable, BadgeTone.Danger)]
    [Arguments(FeedStatus.Invalid, BadgeTone.Danger)]
    public async Task Every_feed_status_has_a_badge(FeedStatus status, BadgeTone tone)
    {
        await Assert.That(Badges.ForFeed(status).Tone).IsEqualTo(tone);
    }

    [Test]
    [Arguments(LlmStatus.Completed, BadgeTone.Neutral)]
    [Arguments(LlmStatus.Truncated, BadgeTone.Warning)]
    [Arguments(LlmStatus.Refused, BadgeTone.Danger)]
    [Arguments(LlmStatus.Unavailable, BadgeTone.Danger)]
    [Arguments(LlmStatus.Rejected, BadgeTone.Danger)]
    public async Task Every_llm_status_has_a_badge(LlmStatus status, BadgeTone tone)
    {
        await Assert.That(Badges.ForLlm(status).Tone).IsEqualTo(tone);
    }
}
