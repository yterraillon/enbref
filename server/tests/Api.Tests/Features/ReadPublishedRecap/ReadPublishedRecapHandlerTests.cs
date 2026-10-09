using EnBref.Api.Features.ReadPublishedRecap;
using EnBref.Api.Shared;

namespace EnBref.Api.Tests.Features.ReadPublishedRecap;

public class ReadPublishedRecapHandlerTests
{
    private static readonly Recap Recap = new(
        new DateOnly(2026, 10, 9),
        Enum.GetValues<Category>().ToDictionary(
            category => category,
            IReadOnlyList<Brief> (category) => [new Brief($"Titre {category}", "Une phrase.")]));

    private static ReadPublishedRecapHandler Handler(StubPublicationRepository repository, int day = 9) =>
        new(repository, new StubTimeProvider(new DateTimeOffset(2026, 10, day, 18, 0, 0, TimeSpan.Zero)));

    [Test]
    [Arguments(9, true)]
    [Arguments(10, false)]
    public async Task Tells_whether_the_published_recap_is_from_today(int today, bool isFromToday)
    {
        var repository = StubPublicationRepository.Succeeding();
        repository.Artifacts["latest.json"] = RecapContract.Serialize(Recap);

        var result = await Handler(repository, today).HandleAsync(RecapType.Daily, CancellationToken.None);

        await Assert.That(result.IsFromToday).IsEqualTo(isFromToday);
    }

    [Test]
    [Arguments(RecapType.Daily, "latest.json")]
    [Arguments(RecapType.Demo, "demo.json")]
    [Arguments(RecapType.Test, "test.json")]
    public async Task Reads_the_artifact_of_the_recap_type(RecapType type, string artifact)
    {
        var repository = StubPublicationRepository.Succeeding();
        repository.Artifacts[artifact] = RecapContract.Serialize(Recap);

        var result = await Handler(repository).HandleAsync(type, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(result.Artifact).IsEqualTo(artifact);
        await Assert.That(result.Recap!.Date).IsEqualTo(Recap.Date);
    }

    [Test]
    public async Task Missing_artifact_is_an_error()
    {
        var result = await Handler(StubPublicationRepository.Succeeding()).HandleAsync(RecapType.Demo, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(result.IsMissing).IsTrue();
        await Assert.That(result.Recap).IsNull();
        await Assert.That(result.Content).IsNull();
    }

    [Test]
    public async Task Artifact_off_contract_keeps_its_raw_content()
    {
        const string content = """{ "title": "Récap du 9 octobre", "sections": [] }""";
        var repository = StubPublicationRepository.Succeeding();
        repository.Artifacts["latest.json"] = content;

        var result = await Handler(repository).HandleAsync(RecapType.Daily, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(result.Error).Contains("latest.json");
        await Assert.That(result.IsMissing).IsFalse();
        await Assert.That(result.Recap).IsNull();
        await Assert.That(result.Content).IsEqualTo(content);
    }
}
