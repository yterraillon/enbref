using EnBref.Api.Features.CollectHeadlines;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Collection;
using EnBref.Infrastructure.Publication;

namespace EnBref.Api.Features.GenerateRecap;

/// <param name="Type">Type de récap à générer.</param>
/// <param name="Publish">Faux : la génération s'arrête avant la publication.</param>
public sealed record GenerateRecapCommand(RecapType Type, bool Publish);

/// <param name="Recap">Null en cas d'échec de la collecte ou de la génération.</param>
/// <param name="Artifact">Artefact visé ; null tant que la publication n'est pas tentée.</param>
/// <param name="Publication">Null si la publication n'est pas demandée ou pas atteinte.</param>
/// <param name="Failure">Null si la génération a réussi.</param>
public sealed record GenerateRecapResult(
    CollectionResult Collection,
    Recap? Recap = null,
    string? Artifact = null,
    PublicationResult? Publication = null,
    GenerateRecapFailure? Failure = null)
{
    public bool IsSuccessful => Failure is null;
}

/// <summary>Étape d'une génération — voir docs/ubiquitous-language.md.</summary>
public enum GenerationStep
{
    Collection,
    Generation,
    Publication,
}

/// <param name="Step">Étape en échec.</param>
/// <param name="Message">Cause lisible, renvoyée telle quelle par l'endpoint.</param>
public sealed record GenerateRecapFailure(GenerationStep Step, string Message);

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
            return Fail(command.Type, new GenerateRecapResult(collection), GenerationStep.Collection, "Aucun titre collecté.");
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
            return Fail(command.Type, new GenerateRecapResult(collection), GenerationStep.Generation, $"Génération impossible : {written.Error}");
        }

        var generated = new GenerateRecapResult(collection, recap);
        return command.Publish ? await PublishAsync(command.Type, pipeline, generated, recap, cancellationToken) : generated;
    }

    // Seul endroit où le type de récap choisit la source et le rédacteur. Le récap de test lit la fausse
    // source, n'appelle pas le LLM et n'atteint que test.json (ADR-007) : l'artefact découle du type.
    private RecapPipeline PipelineFor(RecapType type) => type switch
    {
        RecapType.Daily => new(rssFeedReader, generationAgent, type.Artifact(), "du jour"),
        RecapType.Demo => new(rssFeedReader, generationAgent, type.Artifact(), "de démo"),
        RecapType.Test => new(fakeFeedReader, testRecapWriter, type.Artifact(), "de test"),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Type de récap sans pipeline."),
    };

    private async Task<GenerateRecapResult> PublishAsync(RecapType type, RecapPipeline pipeline, GenerateRecapResult generated, Recap recap, CancellationToken cancellationToken)
    {
        var publication = await publicationRepository.PublishAsync(pipeline.Artifact, RecapContract.Serialize(recap),
            $"Publication du récap {pipeline.Label} du {recap.Date:yyyy-MM-dd}", cancellationToken);
        var published = generated with { Artifact = pipeline.Artifact, Publication = publication };

        return publication.IsSuccessful
            ? published
            : Fail(type, published, GenerationStep.Publication, $"Publication sur {pipeline.Artifact} impossible : {publication.Error}");
    }

    private GenerateRecapResult Fail(RecapType type, GenerateRecapResult result, GenerationStep step, string message)
    {
        // TODO ntfy : génération en échec.
        logger.LogError("Génération {Type} en échec à l'étape {Step} : {Message}", type, step, message);
        return result with { Failure = new GenerateRecapFailure(step, message) };
    }

    private sealed record RecapPipeline(IFeedReader FeedReader, IRecapWriter Writer, string Artifact, string Label);
}
