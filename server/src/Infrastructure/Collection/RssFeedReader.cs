using System.Diagnostics;
using System.ServiceModel.Syndication;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace EnBref.Infrastructure.Collection;

public sealed class RssFeedReader(HttpClient httpClient, ILogger<RssFeedReader> logger) : IFeedReader
{
    public async Task<FeedResult> ReadAsync(Feed feed, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await CollectAsync(feed, cancellationToken);

        if (result.Status == FeedStatus.Available)
        {
            logger.LogInformation("Collecte {Source} : {Status}, {Count} titres en {Elapsed} ms.",
                feed.Source, result.Status, result.Headlines.Count, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            logger.LogWarning("Collecte {Source} : {Status} en {Elapsed} ms. {Error}",
                feed.Source, result.Status, stopwatch.ElapsedMilliseconds, result.Error);
        }

        foreach (var headline in result.Headlines)
        {
            logger.LogDebug("Titre collecté {Source} : {Headline}", feed.Source, headline);
        }

        return result;
    }

    private async Task<FeedResult> CollectAsync(Feed feed, CancellationToken cancellationToken)
    {
        string content;
        try
        {
            content = await httpClient.GetStringAsync(feed.Url, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            return new FeedResult(feed, FeedStatus.Unreachable, [], exception.Message);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            return new FeedResult(feed, FeedStatus.Unreachable, [], $"Timeout : {exception.Message}");
        }

        SyndicationFeed syndicationFeed;
        try
        {
            using var reader = XmlReader.Create(new StringReader(content), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore });
            syndicationFeed = SyndicationFeed.Load(reader);
        }
        catch (Exception exception) when (exception is XmlException or InvalidOperationException)
        {
            return new FeedResult(feed, FeedStatus.Invalid, [], exception.Message);
        }

        // Doublons conservés : ils signalent la fréquence d'un sujet.
        var headlines = syndicationFeed.Items
            .Select(item => item.Title?.Text?.Trim())
            .Where(title => !string.IsNullOrEmpty(title))
            .Select(title => title!)
            .ToList();

        return headlines.Count == 0
            ? new FeedResult(feed, FeedStatus.Empty, [])
            : new FeedResult(feed, FeedStatus.Available, headlines);
    }
}
