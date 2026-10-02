using EnBref.Infrastructure.Collection;

namespace EnBref.Api.Features.CollectHeadlines;

/// <summary>Résultat de collecte — voir docs/ubiquitous-language.md.</summary>
public sealed record CollectionResult(IReadOnlyList<FeedResult> Feeds)
{
    /// <summary>Tous les titres collectés, doublons compris.</summary>
    public IReadOnlyList<string> Headlines { get; } = Feeds.SelectMany(feed => feed.Headlines).ToList();

    public bool IsSuccessful => Headlines.Count > 0;

    public bool IsDegraded => Feeds.Any(feed => feed.Status != FeedStatus.Available);
}
