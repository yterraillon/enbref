using EnBref.Api.Features.CollectHeadlines;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Publication;

namespace EnBref.Api.Features.GenerateRecap;

public sealed class GenerateRecapHandler(
    CollectHeadlinesHandler collectHeadlines,
    GenerationAgent generationAgent,
    IPublicationRepository publicationRepository,
    TimeProvider timeProvider,
    ILogger<GenerateRecapHandler> logger)
{
    public async Task<GenerateRecapResult> HandleAsync(GenerateRecapCommand command, CancellationToken cancellationToken)
    {
        // Le récap de test lit une fausse source : il ne dépend pas des flux réels.
        var collection = await collectHeadlines.HandleAsync(command.Type == RecapType.Test, cancellationToken);

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

        // Le récap de test ne consomme pas de crédits : il n'appelle pas le LLM.
        if (command.Type == RecapType.Test)
        {
            return await PublishAsync(command, new GenerateRecapResult(collection, TestRecap.From(date, collection.Headlines), Error: null), cancellationToken);
        }

        var generation = await generationAgent.WriteAsync(new GenerationContext(date, collection.Headlines), cancellationToken);
        if (!generation.IsSuccessful)
        {
            // TODO ntfy : génération impossible.
            logger.LogError("Génération {Type} impossible : {Error}", command.Type, generation.Error);
            return new GenerateRecapResult(collection, Recap: null, generation.Error);
        }

        return await PublishAsync(command, new GenerateRecapResult(collection, generation.Recap, Error: null), cancellationToken);
    }

    private async Task<GenerateRecapResult> PublishAsync(GenerateRecapCommand command, GenerateRecapResult generated, CancellationToken cancellationToken)
    {
        if (!command.Publish || generated.Recap is not { } recap)
        {
            return generated;
        }

        var artifact = RecapArtifact.For(command.Type);
        var publication = await publicationRepository.PublishAsync(artifact, RecapContract.Serialize(recap),
            $"Publication du récap {Label(command.Type)} du {recap.Date:yyyy-MM-dd}", cancellationToken);
        if (!publication.IsSuccessful)
        {
            // TODO ntfy : publication impossible.
            logger.LogError("Publication {Type} sur {Artifact} impossible : {Error}", command.Type, artifact, publication.Error);
        }

        return generated with { Artifact = artifact, Publication = publication };
    }

    private static string Label(RecapType type) => type switch
    {
        RecapType.Daily => "du jour",
        RecapType.Demo => "de démo",
        RecapType.Test => "de test",
        _ => type.ToString(),
    };
}
