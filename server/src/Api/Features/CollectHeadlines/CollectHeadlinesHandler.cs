using EnBref.Infrastructure.Collection;
using Microsoft.Extensions.Options;

namespace EnBref.Api.Features.CollectHeadlines;

/// <summary>Collecte : partagée par la génération et le back-office.</summary>
public sealed class CollectHeadlinesHandler(IOptions<FeedOptions> options)
{
    /// <param name="reader">Lecteur réel ou fausse source : c'est l'appelant qui choisit.</param>
    public async Task<CollectionResult> HandleAsync(IFeedReader reader, CancellationToken cancellationToken)
    {
        var results = await Task.WhenAll(options.Value.Feeds.Select(feed => reader.ReadAsync(feed, cancellationToken)));
        return new CollectionResult(results);
    }
}
