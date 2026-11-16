using System.Xml.Linq;
using EnBref.Api.Features.PublishRecapFeed;
using EnBref.Infrastructure.Publication;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnBref.Api.Tests.Features.PublishRecapFeed;

public class PublishRecapFeedHandlerTests
{
    private static readonly DateOnly Today = new(2026, 11, 16);

    private readonly StubPublicationRepository repository = StubPublicationRepository.Succeeding();
    private readonly StubAlertSender alerts = new();

    internal static PublishRecapFeedHandler Handler(IPublicationRepository repository, StubAlertSender alerts) =>
        new(repository, alerts, new StubTimeProvider(new DateTimeOffset(2026, 11, 16, 16, 0, 0, TimeSpan.Zero)),
            NullLogger<PublishRecapFeedHandler>.Instance);

    private string[] PublishedGuids() => XDocument.Parse(repository.Publications.Single().Content)
        .Root!.Element("channel")!.Elements("item").Select(item => item.Element("guid")!.Value).ToArray();

    [Test]
    public async Task Missing_feed_is_published_with_the_recap_of_the_day()
    {
        var result = await Handler(repository, alerts).HandleAsync(RecapFeedTests.Recap(Today), CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(repository.Publications.Single().Artifact).IsEqualTo("feed.xml");
        await Assert.That(PublishedGuids()).IsEquivalentTo(["enbref-2026-11-16"]);
        await Assert.That(alerts.Alerts).IsEmpty();
    }

    [Test]
    public async Task Published_feed_keeps_its_previous_items()
    {
        var yesterday = RecapFeed.Item(RecapFeedTests.Recap(Today.AddDays(-1)), DateTimeOffset.UnixEpoch);
        repository.Artifacts["feed.xml"] = RecapFeed.Merge(null, yesterday, DateTimeOffset.UnixEpoch);

        await Handler(repository, alerts).HandleAsync(RecapFeedTests.Recap(Today), CancellationToken.None);

        await Assert.That(PublishedGuids()).IsEquivalentTo(["enbref-2026-11-16", "enbref-2026-11-15"]);
    }

    [Test]
    public async Task Unreadable_feed_starts_over()
    {
        repository.Artifacts["feed.xml"] = "pas du xml";

        var result = await Handler(repository, alerts).HandleAsync(RecapFeedTests.Recap(Today), CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(PublishedGuids()).IsEquivalentTo(["enbref-2026-11-16"]);
    }

    [Test]
    public async Task Failed_read_publishes_nothing_and_raises_an_alert()
    {
        repository.ReadErrors["feed.xml"] = "GitHub 503";

        var result = await Handler(repository, alerts).HandleAsync(RecapFeedTests.Recap(Today), CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(repository.Publications).IsEmpty();
        await Assert.That(alerts.Alerts.Single().Message).Contains("GitHub 503");
    }

    [Test]
    public async Task Failed_publication_raises_an_alert()
    {
        repository.Results["feed.xml"] = new PublicationResult(CommitUrl: null, "GitHub 409");

        var result = await Handler(repository, alerts).HandleAsync(RecapFeedTests.Recap(Today), CancellationToken.None);

        await Assert.That(result.Error).Contains("GitHub 409");
        var alert = alerts.Alerts.Single();
        await Assert.That(alert.Title).IsEqualTo("Flux du récap non publié");
        await Assert.That(alert.Message).Contains("GitHub 409");
    }
}
