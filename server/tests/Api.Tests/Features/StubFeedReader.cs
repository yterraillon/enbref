using EnBref.Infrastructure.Collection;

namespace EnBref.Api.Tests.Features;

/// <summary>Lecteur de flux qui renvoie un état choisi par flux et compte ses appels.</summary>
internal sealed class StubFeedReader(Func<Feed, FeedResult> read) : IFeedReader
{
    public int Calls { get; private set; }

    public static StubFeedReader Available(params string[] headlines) =>
        new(feed => new FeedResult(feed, FeedStatus.Available, headlines));

    public Task<FeedResult> ReadAsync(Feed feed, CancellationToken cancellationToken)
    {
        Calls++;
        return Task.FromResult(read(feed));
    }
}
