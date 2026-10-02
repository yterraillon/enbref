using EnBref.Infrastructure.Collection;
using Microsoft.Extensions.Options;

namespace EnBref.Api.Features.CollectHeadlines;

/// <summary>Collecte : partagée par la génération et le back-office.</summary>
public sealed class CollectHeadlinesHandler(
    [FromKeyedServices(FeedReaderKeys.Rss)] IFeedReader rssFeedReader,
    [FromKeyedServices(FeedReaderKeys.Fake)] IFeedReader fakeFeedReader,
    IOptions<FeedOptions> options)
{
    public async Task<CollectionResult> HandleAsync(bool useFakeReader, CancellationToken cancellationToken)
    {
        var reader = useFakeReader ? fakeFeedReader : rssFeedReader;
        var results = await Task.WhenAll(options.Value.Feeds.Select(feed => reader.ReadAsync(feed, cancellationToken)));
        return new CollectionResult(results);
    }
}
