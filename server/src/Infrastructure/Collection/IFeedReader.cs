namespace EnBref.Infrastructure.Collection;

public static class FeedReaderKeys
{
    public const string Rss = "rss";
    public const string Fake = "fake";
}

public interface IFeedReader
{
    /// <summary>Constate l'état du flux sans jamais lever, hors annulation.</summary>
    Task<FeedResult> ReadAsync(Feed feed, CancellationToken cancellationToken);
}
