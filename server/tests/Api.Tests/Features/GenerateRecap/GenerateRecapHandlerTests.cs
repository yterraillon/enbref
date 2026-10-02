using EnBref.Api.Features.GenerateRecap;
using EnBref.Api.Shared;
using EnBref.Api.Tests.Features.CollectHeadlines;
using EnBref.Infrastructure.Collection;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnBref.Api.Tests.Features.GenerateRecap;

public class GenerateRecapHandlerTests
{
    private static readonly Feed Feed = new("Source", new Uri("https://source.test/rss"));

    private static GenerateRecapHandler Handler(IFeedReader rss, IFeedReader fake) =>
        new(CollectHeadlinesHandlerTests.Handler(rss, fake, Feed), NullLogger<GenerateRecapHandler>.Instance);

    [Test]
    [Arguments(RecapType.Daily, "réel")]
    [Arguments(RecapType.Demo, "réel")]
    [Arguments(RecapType.Test, "faux")]
    public async Task Only_the_test_recap_reads_the_fake_source(RecapType type, string expectedHeadline)
    {
        var handler = Handler(StubFeedReader.Available("réel"), StubFeedReader.Available("faux"));

        var result = await handler.HandleAsync(new GenerateRecapCommand(type, Publish: false), CancellationToken.None);

        await Assert.That(result.Collection.Headlines).IsEquivalentTo([expectedHeadline]);
    }

    [Test]
    public async Task Failed_collection_fails_the_generation()
    {
        var broken = new StubFeedReader(feed => new FeedResult(feed, FeedStatus.Invalid, [], "pas du XML"));
        var handler = Handler(broken, StubFeedReader.Available("faux"));

        var result = await handler.HandleAsync(new GenerateRecapCommand(RecapType.Daily, Publish: false), CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
    }
}
