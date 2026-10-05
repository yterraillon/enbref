using EnBref.Api.Features.CollectHeadlines;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Llm;

namespace EnBref.Api.Features.GenerateRecap;

public sealed class GenerateRecapHandler(
    CollectHeadlinesHandler collectHeadlines,
    ILlmClient llmClient,
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
            return new GenerateRecapResult(collection, LlmResponse: null);
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
            return new GenerateRecapResult(collection, LlmResponse: null);
        }

        var llmResponse = await llmClient.SendAsync(cancellationToken);
        return new GenerateRecapResult(collection, llmResponse);
    }
}
