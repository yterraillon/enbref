using EnBref.Api.Features.CollectHeadlines;
using EnBref.Api.Shared;

namespace EnBref.Api.Features.GenerateRecap;

public sealed class GenerateRecapHandler(
    CollectHeadlinesHandler collectHeadlines,
    GenerationAgent generationAgent,
    TimeProvider timeProvider,
    ILogger<GenerateRecapHandler> logger)
{
    // Étape suivante : publication, sur l'artefact du type — test.json seulement pour un RecapType.Test.
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

        // Le récap de test ne consomme pas de crédits : il n'appelle pas le LLM.
        if (command.Type == RecapType.Test)
        {
            return new GenerateRecapResult(collection, Recap: null, Error: null);
        }

        var date = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var generation = await generationAgent.WriteAsync(new GenerationContext(date, collection.Headlines), cancellationToken);
        if (!generation.IsSuccessful)
        {
            // TODO ntfy : génération impossible.
            logger.LogError("Génération {Type} impossible : {Error}", command.Type, generation.Error);
        }

        return new GenerateRecapResult(collection, generation.Recap, generation.Error);
    }
}
