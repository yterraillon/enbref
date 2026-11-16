using EnBref.Api.Features.GenerateDailyRecap;
using EnBref.Api.Tests.Features.GenerateRecap;
using EnBref.Infrastructure.Publication;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnBref.Api.Tests.Features.GenerateDailyRecap;

public class GenerateDailyRecapHandlerTests
{
    private static readonly PublicationResult Published = new(new Uri("https://github.test/commit/1"), Error: null);
    private static readonly PublicationResult Rejected = new(CommitUrl: null, "401 Unauthorized");

    private readonly TemporarySettings settings = new();
    private readonly StubAlertSender alerts = new();

    [After(Test)]
    public void DeleteSettings() => settings.Dispose();

    private GenerateDailyRecapHandler Handler(SequencedPublicationRepository repository, bool isStarted = true) =>
        new(GenerateRecapHandlerTests.Handler(StubFeedReader.Available("réel"), StubFeedReader.Available("faux"), publicationRepository: repository),
            settings.Store(isStarted),
            alerts,
            new DailyRecapRetry(3, TimeSpan.Zero),
            TimeProvider.System,
            NullLogger<GenerateDailyRecapHandler>.Instance);

    [Test]
    public async Task Stopped_daily_generation_generates_nothing()
    {
        var repository = new SequencedPublicationRepository(Published);

        var result = await Handler(repository, isStarted: false).HandleAsync(CancellationToken.None);

        await Assert.That(result.IsSkipped).IsTrue();
        await Assert.That(repository.Publications).IsEmpty();
        await Assert.That(alerts.Alerts).IsEmpty();
    }

    [Test]
    public async Task Daily_recap_is_published_on_latest()
    {
        var repository = new SequencedPublicationRepository(Published);

        var result = await Handler(repository).HandleAsync(CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(repository.Publications).IsEquivalentTo(["latest.json"]);
    }

    [Test]
    public async Task A_failed_attempt_is_retried_without_alert()
    {
        var repository = new SequencedPublicationRepository(Rejected, Published);

        var result = await Handler(repository).HandleAsync(CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(result.Attempts).IsEqualTo(2);
        await Assert.That(alerts.Alerts).IsEmpty();
    }

    [Test]
    public async Task Three_failed_attempts_raise_one_alert()
    {
        var repository = new SequencedPublicationRepository(Rejected, Rejected, Rejected);

        var result = await Handler(repository).HandleAsync(CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(repository.Publications).Count().IsEqualTo(3);
        var alert = alerts.Alerts.Single();
        await Assert.That(alert.Title).IsEqualTo("Récap du jour non publié");
        await Assert.That(alert.Message).Contains("Erreur lors de la publication, après 3 tentatives");
        await Assert.That(alert.Message).Contains("401 Unauthorized");
    }

    /// <summary>Dépôt de publication qui renvoie les résultats choisis, dans l'ordre.</summary>
    internal sealed class SequencedPublicationRepository(params PublicationResult[] results) : IPublicationRepository
    {
        public List<string> Publications { get; } = [];

        public Task<PublicationResult> PublishAsync(string artifact, string content, string message, CancellationToken cancellationToken)
        {
            Publications.Add(artifact);
            return Task.FromResult(results[Publications.Count - 1]);
        }

        public Task<ArtifactReadResult> ReadAsync(string artifact, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
