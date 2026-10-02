namespace EnBref.Infrastructure.Collection;

public sealed record FeedResult(Feed Feed, FeedStatus Status, IReadOnlyList<string> Headlines, string? Error = null);
