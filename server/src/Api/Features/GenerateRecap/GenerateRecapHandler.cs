using EnBref.Api.Features.CollectHeadlines;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Collection;
using EnBref.Infrastructure.Publication;

namespace EnBref.Api.Features.GenerateRecap;

public sealed class GenerateRecapHandler(
    CollectHeadlinesHandler collectHeadlines,
    [FromKeyedServices(FeedReaderKeys.Rss)] IFeedReader rssFeedReader,
    [FromKeyedServices(FeedReaderKeys.Fake)] IFeedReader fakeFeedReader,
    GenerationAgent generationAgent,
    TestRecapWriter testRecapWriter,
    IPublicationRepository publicationRepository,
    TimeProvider timeProvider,
    ILogger<GenerateRecapHandler> logger)
{
    public async Task<GenerateRecapResult> HandleAsync(GenerateRecapCommand command, CancellationToken cancellationToken)
    {
        var pipeline = PipelineFor(command.Type);

        var collection = await collectHeadlines.HandleAsync(pipeline.FeedReader, cancellationToken);
        if (!collection.IsSuccessful)
        {
            // TODO ntfy : génération impossible.
            logger.LogError("Génération {Type} impossible : aucun titre collecté.", command.Type);
            return new GenerateRecapResult(collection, Recap: null, Error: null);
        }

        if (collection.IsDegraded)
        {
            // TODO ntfy : flux dégradé.
            logger.LogWarning("Collecte dégradée pour la génération {Type} : {Feeds}.", command.Type,
                string.Join(", ", collection.Feeds.Select(feed => $"{feed.Feed.Source} {feed.Status}")));
        }

        var date = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var written = await pipeline.Writer.WriteAsync(new GenerationContext(date, collection.Headlines), cancellationToken);
        if (written.Recap is not { } recap)
        {
            // TODO ntfy : génération impossible.
            logger.LogError("Génération {Type} impossible : {Error}", command.Type, written.Error);
            return new GenerateRecapResult(collection, Recap: null, written.Error);
        }

        var generated = new GenerateRecapResult(collection, recap, Error: null);
        return command.Publish ? await PublishAsync(pipeline, generated, recap, cancellationToken) : generated;
    }

    // Seul endroit où le type de récap est lu : le récap de test lit la fausse source et n'appelle pas le LLM (ADR-007).
    private RecapPipeline PipelineFor(RecapType type) => type switch
    {
        RecapType.Daily => new(rssFeedReader, generationAgent, RecapArtifact.For(type), "du jour"),
        RecapType.Demo => new(rssFeedReader, generationAgent, RecapArtifact.For(type), "de démo"),
        RecapType.Test => new(fakeFeedReader, testRecapWriter, RecapArtifact.For(type), "de test"),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Type de récap sans pipeline."),
    };

    private async Task<GenerateRecapResult> PublishAsync(RecapPipeline pipeline, GenerateRecapResult generated, Recap recap, CancellationToken cancellationToken)
    {
        var publication = await publicationRepository.PublishAsync(pipeline.Artifact, RecapContract.Serialize(recap),
            $"Publication du récap {pipeline.Label} du {recap.Date:yyyy-MM-dd}", cancellationToken);
        if (!publication.IsSuccessful)
        {
            // TODO ntfy : publication impossible.
            logger.LogError("Publication sur {Artifact} impossible : {Error}", pipeline.Artifact, publication.Error);
        }

        return generated with { Artifact = pipeline.Artifact, Publication = publication };
    }

    private sealed record RecapPipeline(IFeedReader FeedReader, IRecapWriter Writer, string Artifact, string Label);
}
