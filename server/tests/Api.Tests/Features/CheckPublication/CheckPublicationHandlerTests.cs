using EnBref.Api.Features.CheckPublication;
using EnBref.Api.Features.PublishRecapFeed;
using EnBref.Api.Features.ReadPublishedRecap;
using EnBref.Api.Shared;
using EnBref.Api.Tests.Features.PublishRecapFeed;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnBref.Api.Tests.Features.CheckPublication;

public class CheckPublicationHandlerTests
{
    private static readonly DateOnly Today = new(2026, 11, 16);

    private readonly TemporarySettings settings = new();
    private readonly StubAlertSender alerts = new();
    private readonly StubPublicationRepository repository = StubPublicationRepository.Succeeding();

    [After(Test)]
    public void DeleteSettings() => settings.Dispose();

    private CheckPublicationHandler Handler(bool isStarted = true) =>
        new(new ReadPublishedRecapHandler(repository, new StubTimeProvider(new DateTimeOffset(2026, 11, 16, 18, 0, 0, TimeSpan.Zero))),
            repository,
            settings.Store(isStarted),
            alerts,
            NullLogger<CheckPublicationHandler>.Instance);

    // Publie le récap du jour et, sauf mention contraire, le flux du récap à la même date.
    private void Publish(DateOnly date, DateOnly? feedDate = null, bool withFeed = true)
    {
        repository.Artifacts["latest.json"] = RecapContract.Serialize(RecapFeedTests.Recap(date));
        if (withFeed)
        {
            var item = RecapFeed.Item(RecapFeedTests.Recap(feedDate ?? date), DateTimeOffset.UnixEpoch);
            repository.Artifacts["feed.xml"] = RecapFeed.Merge(existing: null, item, DateTimeOffset.UnixEpoch);
        }
    }

    [Test]
    public async Task Recap_published_for_the_day_raises_no_alert()
    {
        Publish(Today);

        var result = await Handler().HandleAsync(Today, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(alerts.Alerts).IsEmpty();
    }

    [Test]
    public async Task Missing_recap_raises_an_alert()
    {
        var result = await Handler().HandleAsync(Today, CancellationToken.None);

        await Assert.That(result.Problem).Contains("absent");
        await Assert.That(alerts.Alerts.Single().Title).IsEqualTo("Récap du jour absent");
    }

    [Test]
    public async Task Recap_from_a_previous_day_raises_an_alert()
    {
        Publish(Today.AddDays(-1));

        var result = await Handler().HandleAsync(Today, CancellationToken.None);

        await Assert.That(result.Problem).IsEqualTo("latest.json est daté du 15/11/2026, le récap du 16/11/2026 n'a pas été publié.");
        await Assert.That(alerts.Alerts).Count().IsEqualTo(1);
    }

    [Test]
    public async Task Unreadable_recap_raises_an_alert()
    {
        repository.Artifacts["latest.json"] = "{ pas du json";

        await Handler().HandleAsync(Today, CancellationToken.None);

        var alert = alerts.Alerts.Single();
        await Assert.That(alert.Title).IsEqualTo("Récap du jour non vérifié");
        await Assert.That(alert.Message).Contains("illisible");
    }

    [Test]
    public async Task Missing_feed_raises_an_alert()
    {
        Publish(Today, withFeed: false);

        var result = await Handler().HandleAsync(Today, CancellationToken.None);

        await Assert.That(result.Problem).IsEqualTo("feed.xml est absent du dépôt de publication.");
        await Assert.That(alerts.Alerts.Single().Title).IsEqualTo("Flux du récap absent");
    }

    [Test]
    public async Task Feed_without_the_recap_of_the_day_raises_an_alert()
    {
        Publish(Today, feedDate: Today.AddDays(-1));

        var result = await Handler().HandleAsync(Today, CancellationToken.None);

        await Assert.That(result.Problem).IsEqualTo("Le dernier item de feed.xml est daté du 15/11/2026, le récap du 16/11/2026 n'y est pas.");
        await Assert.That(alerts.Alerts.Single().Title).IsEqualTo("Flux du récap absent");
    }

    [Test]
    public async Task Unreadable_feed_raises_an_alert()
    {
        Publish(Today, withFeed: false);
        repository.Artifacts["feed.xml"] = "pas du xml";

        await Handler().HandleAsync(Today, CancellationToken.None);

        await Assert.That(alerts.Alerts.Single().Title).IsEqualTo("Flux du récap non vérifié");
    }

    [Test]
    public async Task Missing_recap_does_not_check_the_feed()
    {
        await Handler().HandleAsync(Today, CancellationToken.None);

        await Assert.That(alerts.Alerts.Select(alert => alert.Title)).IsEquivalentTo(["Récap du jour absent"]);
    }

    [Test]
    public async Task Stopped_daily_generation_checks_nothing()
    {
        var result = await Handler(isStarted: false).HandleAsync(Today, CancellationToken.None);

        await Assert.That(result.IsSkipped).IsTrue();
        await Assert.That(alerts.Alerts).IsEmpty();
    }
}
