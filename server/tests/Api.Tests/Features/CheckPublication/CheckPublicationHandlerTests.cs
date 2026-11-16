using EnBref.Api.Features.CheckPublication;
using EnBref.Api.Features.ReadPublishedRecap;
using EnBref.Api.Shared;
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
            settings.Store(isStarted),
            alerts,
            NullLogger<CheckPublicationHandler>.Instance);

    private void Publish(DateOnly date) => repository.Artifacts["latest.json"] = RecapContract.Serialize(new Recap(
        date,
        Enum.GetValues<Category>().ToDictionary(
            category => category,
            IReadOnlyList<Brief> (category) => [new Brief($"Titre {category}", "Une phrase.")])));

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

        await Assert.That(alerts.Alerts.Single().Message).Contains("illisible");
    }

    [Test]
    public async Task Stopped_daily_generation_checks_nothing()
    {
        var result = await Handler(isStarted: false).HandleAsync(Today, CancellationToken.None);

        await Assert.That(result.IsSkipped).IsTrue();
        await Assert.That(alerts.Alerts).IsEmpty();
    }
}
