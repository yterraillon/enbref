using EnBref.Api.Features.CollectHeadlines;
using EnBref.Infrastructure.Collection;
using EnBref.Infrastructure.Publication;

namespace EnBref.Api.Features.GenerateRecap;

/// <param name="Type">Type de récap à générer.</param>
/// <param name="Publish">Faux : la génération s'arrête avant la publication.</param>
public sealed record GenerateRecapCommand(RecapType Type, bool Publish);

/// <param name="Recap">Null en cas d'échec de la collecte ou de la génération.</param>
/// <param name="Error">Cause de l'échec de la génération, après une collecte réussie.</param>
/// <param name="Artifact">Artefact visé ; null tant que la publication n'est pas tentée.</param>
/// <param name="Publication">Null si la publication n'est pas demandée ou pas atteinte.</param>
public sealed record GenerateRecapResult(
    CollectionResult Collection,
    Recap? Recap,
    string? Error,
    string? Artifact = null,
    PublicationResult? Publication = null)
{
    public bool IsSuccessful => Collection.IsSuccessful && Error is null && Publication?.IsSuccessful != false;
}

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

    // Seul endroit où le type de récap est lu, et seul endroit qui nomme un artefact publié. L'artefact
    // découle du type, jamais d'un paramètre d'appel ni de la configuration : le récap de test lit la
    // fausse source, n'appelle pas le LLM et n'atteint que test.json (ADR-007).
    private RecapPipeline PipelineFor(RecapType type) => type switch
    {
        RecapType.Daily => new(rssFeedReader, generationAgent, "latest.json", "du jour"),
        RecapType.Demo => new(rssFeedReader, generationAgent, "demo.json", "de démo"),
        RecapType.Test => new(fakeFeedReader, testRecapWriter, "test.json", "de test"),
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
