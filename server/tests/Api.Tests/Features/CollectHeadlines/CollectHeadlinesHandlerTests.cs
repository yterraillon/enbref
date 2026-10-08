using EnBref.Api.Features.CollectHeadlines;
using EnBref.Infrastructure.Collection;
using Microsoft.Extensions.Options;

namespace EnBref.Api.Tests.Features.CollectHeadlines;

public class CollectHeadlinesHandlerTests
{
    private static readonly Feed Working = new("Working", new Uri("https://working.test/rss"));
    private static readonly Feed Broken = new("Broken", new Uri("https://broken.test/rss"));

    internal static CollectHeadlinesHandler Handler(params Feed[] feeds) =>
        new(Options.Create(new FeedOptions { Feeds = [.. feeds] }));

    [Test]
    public async Task One_broken_feed_still_collects_the_others()
    {
        var rss = new StubFeedReader(feed => feed == Broken
            ? new FeedResult(feed, FeedStatus.Unreachable, [], "timeout")
            : new FeedResult(feed, FeedStatus.Available, ["A", "B"]));

        var result = await Handler(Working, Broken).HandleAsync(rss, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(result.IsDegraded).IsTrue();
        await Assert.That(result.Headlines).IsEquivalentTo(["A", "B"]);
    }

    [Test]
    public async Task All_broken_feeds_fail_the_collection()
    {
        var rss = new StubFeedReader(feed => new FeedResult(feed, FeedStatus.Empty, []));

        var result = await Handler(Working, Broken).HandleAsync(rss, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
    }

    [Test]
    public async Task Duplicate_headlines_across_feeds_are_kept()
    {
        var rss = StubFeedReader.Available("Même dépêche");

        var result = await Handler(Working, Broken).HandleAsync(rss, CancellationToken.None);

        await Assert.That(result.Headlines).IsEquivalentTo(["Même dépêche", "Même dépêche"]);
    }
}
