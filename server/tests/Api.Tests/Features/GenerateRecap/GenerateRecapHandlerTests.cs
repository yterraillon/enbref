using EnBref.Api.Features.GenerateRecap;
using EnBref.Api.Shared;
using EnBref.Api.Tests.Features.CollectHeadlines;
using EnBref.Infrastructure.Collection;
using EnBref.Infrastructure.Llm;
using EnBref.Infrastructure.Publication;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnBref.Api.Tests.Features.GenerateRecap;

public class GenerateRecapHandlerTests
{
    private static readonly Feed Feed = new("Source", new Uri("https://source.test/rss"));

    private static GenerateRecapHandler Handler(IFeedReader rss, IFeedReader fake, StubLlmClient? llm = null, StubPublicationRepository? publicationRepository = null) =>
        new(CollectHeadlinesHandlerTests.Handler(Feed),
            rss,
            fake,
            new GenerationAgent(llm ?? StubLlmClient.Completed(GenerationAgentTests.Output()), NullLogger<GenerationAgent>.Instance),
            new TestRecapWriter(),
            publicationRepository ?? StubPublicationRepository.Succeeding(),
            TimeProvider.System,
            NullLogger<GenerateRecapHandler>.Instance);

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
        await Assert.That(result.Failure!.Step).IsEqualTo(GenerationStep.Collection);
    }

    [Test]
    [Arguments(RecapType.Daily)]
    [Arguments(RecapType.Demo)]
    public async Task Successful_collection_returns_the_written_recap(RecapType type)
    {
        var handler = Handler(StubFeedReader.Available("réel"), StubFeedReader.Available("faux"));

        var result = await handler.HandleAsync(new GenerateRecapCommand(type, Publish: false), CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(result.Recap).IsNotNull();
    }

    [Test]
    public async Task Failed_agent_fails_the_generation()
    {
        var llm = new StubLlmClient(new LlmResponse(LlmStatus.Unavailable, "", "surcharge"));
        var handler = Handler(StubFeedReader.Available("réel"), StubFeedReader.Available("faux"), llm);

        var result = await handler.HandleAsync(new GenerateRecapCommand(RecapType.Daily, Publish: false), CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(result.Failure!.Step).IsEqualTo(GenerationStep.Generation);
        await Assert.That(result.Failure.Message).Contains("surcharge");
    }

    [Test]
    public async Task Failed_collection_does_not_call_the_llm()
    {
        var broken = new StubFeedReader(feed => new FeedResult(feed, FeedStatus.Invalid, [], "pas du XML"));
        var llm = StubLlmClient.Completed(GenerationAgentTests.Output());
        var handler = Handler(broken, StubFeedReader.Available("faux"), llm);

        await handler.HandleAsync(new GenerateRecapCommand(RecapType.Daily, Publish: false), CancellationToken.None);

        await Assert.That(llm.Calls).IsEqualTo(0);
    }

    [Test]
    public async Task Test_recap_does_not_call_the_llm()
    {
        var llm = StubLlmClient.Completed(GenerationAgentTests.Output());
        var handler = Handler(StubFeedReader.Available("réel"), StubFeedReader.Available("faux"), llm);

        var result = await handler.HandleAsync(new GenerateRecapCommand(RecapType.Test, Publish: false), CancellationToken.None);

        await Assert.That(llm.Calls).IsEqualTo(0);
        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(result.Recap!.Briefs.Keys).IsEquivalentTo(Enum.GetValues<Category>());
    }

    [Test]
    [Arguments(RecapType.Daily, "latest.json")]
    [Arguments(RecapType.Demo, "demo.json")]
    [Arguments(RecapType.Test, "test.json")]
    public async Task Each_recap_type_is_published_on_its_own_artifact_only(RecapType type, string expectedArtifact)
    {
        var publicationRepository = StubPublicationRepository.Succeeding();
        var handler = Handler(StubFeedReader.Available("réel"), StubFeedReader.Available("faux"), publicationRepository: publicationRepository);

        var result = await handler.HandleAsync(new GenerateRecapCommand(type, Publish: true), CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(publicationRepository.Publications.Select(publication => publication.Artifact)).IsEquivalentTo([expectedArtifact]);
    }

    [Test]
    public async Task Unpublished_generation_does_not_call_the_publication_repository()
    {
        var publicationRepository = StubPublicationRepository.Succeeding();
        var handler = Handler(StubFeedReader.Available("réel"), StubFeedReader.Available("faux"), publicationRepository: publicationRepository);

        var result = await handler.HandleAsync(new GenerateRecapCommand(RecapType.Daily, Publish: false), CancellationToken.None);

        await Assert.That(publicationRepository.Publications).IsEmpty();
        await Assert.That(result.Publication).IsNull();
    }

    [Test]
    public async Task Failed_agent_publishes_nothing()
    {
        var llm = new StubLlmClient(new LlmResponse(LlmStatus.Unavailable, "", "surcharge"));
        var publicationRepository = StubPublicationRepository.Succeeding();
        var handler = Handler(StubFeedReader.Available("réel"), StubFeedReader.Available("faux"), llm, publicationRepository);

        await handler.HandleAsync(new GenerateRecapCommand(RecapType.Daily, Publish: true), CancellationToken.None);

        await Assert.That(publicationRepository.Publications).IsEmpty();
    }

    [Test]
    public async Task Failed_publication_fails_the_generation()
    {
        var publicationRepository = new StubPublicationRepository(new PublicationResult(CommitUrl: null, "GitHub 422"));
        var handler = Handler(StubFeedReader.Available("réel"), StubFeedReader.Available("faux"), publicationRepository: publicationRepository);

        var result = await handler.HandleAsync(new GenerateRecapCommand(RecapType.Daily, Publish: true), CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(result.Failure!.Step).IsEqualTo(GenerationStep.Publication);
        await Assert.That(result.Failure.Message).Contains("GitHub 422");
        await Assert.That(result.Recap).IsNotNull();
    }
}
