namespace EnBref.Infrastructure.Collection;

/// <summary>État du flux — voir docs/ubiquitous-language.md.</summary>
public enum FeedStatus
{
    Available,
    Unreachable,
    Invalid,
    Empty,
}
