using EnBref.Api.Features.CollectHeadlines;
using EnBref.Api.Shared;

namespace EnBref.Api.Features.GenerateRecap;

public sealed class GenerateRecapHandler(CollectHeadlinesHandler collectHeadlines, ILogger<GenerateRecapHandler> logger)
{
    // Étapes suivantes : génération, puis publication — jamais pour un RecapType.Test.
    public async Task<GenerateRecapResult> HandleAsync(GenerateRecapCommand command, CancellationToken cancellationToken)
    {
        // Le récap de test lit une fausse source : il ne dépend pas des flux réels.
        var collection = await collectHeadlines.HandleAsync(command.Type == RecapType.Test, cancellationToken);

        if (!collection.IsSuccessful)
        {
            // TODO ntfy : génération impossible.
            logger.LogError("Génération {Type} impossible : aucun titre collecté.", command.Type);
            return new GenerateRecapResult(collection);
        }

        if (collection.IsDegraded)
        {
            // TODO ntfy : flux dégradé.
            logger.LogWarning("Collecte dégradée pour la génération {Type} : {Feeds}.", command.Type,
                string.Join(", ", collection.Feeds.Select(feed => $"{feed.Feed.Source} {feed.Status}")));
        }

        return new GenerateRecapResult(collection);
    }
}
