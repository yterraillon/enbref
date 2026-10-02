namespace EnBref.Infrastructure.Collection;

/// <summary>Flux RSS publié par une source — voir docs/ubiquitous-language.md.</summary>
public sealed record Feed(string Source, Uri Url);

public sealed class FeedOptions
{
    public const string SectionName = "Collection";

    public List<Feed> Feeds { get; init; } = [];
}
