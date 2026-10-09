using EnBref.Api.Features.CollectHeadlines;
using EnBref.Api.Features.ReadPublishedRecap;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Collection;
using EnBref.Infrastructure.Llm;

namespace EnBref.Api.BackOffice.Display;

/// <summary>Traduit les résultats des handlers en badges du back-office.</summary>
public static class Badges
{
    /// <summary>État d'un artefact lu : un récap de test absent n'a rien d'anormal.</summary>
    public static Badge ForArtifact(RecapType type, ReadPublishedRecapResult result) => result switch
    {
        { IsSuccessful: true } => new Badge("Disponible", BadgeTone.Success),
        { IsMissing: true } => new Badge("Absent", type == RecapType.Test ? BadgeTone.Neutral : BadgeTone.Warning),
        _ => new Badge("Illisible", BadgeTone.Danger),
    };

    /// <summary>« 2 sur 2 flux disponibles » : vert si tous, orange si une partie, rouge si aucun.</summary>
    public static Badge ForCollection(CollectionResult collection)
    {
        var available = collection.Feeds.Count(feed => feed.Status == FeedStatus.Available);
        var tone = available == collection.Feeds.Count ? BadgeTone.Success
            : available > 0 ? BadgeTone.Warning
            : BadgeTone.Danger;
        return new Badge($"{available} sur {collection.Feeds.Count} flux disponibles", tone);
    }

    public static Badge ForFeed(FeedStatus status) => status switch
    {
        FeedStatus.Available => new Badge("Disponible", BadgeTone.Success),
        FeedStatus.Empty => new Badge("Vide", BadgeTone.Warning),
        FeedStatus.Unreachable => new Badge("Injoignable", BadgeTone.Danger),
        FeedStatus.Invalid => new Badge("Illisible", BadgeTone.Danger),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "État du flux sans badge."),
    };

    public static Badge ForLlm(LlmStatus status) => status switch
    {
        LlmStatus.Completed => new Badge("Envoyé", BadgeTone.Neutral),
        LlmStatus.Truncated => new Badge("Tronquée", BadgeTone.Warning),
        LlmStatus.Refused => new Badge("Refusé", BadgeTone.Danger),
        LlmStatus.Unavailable => new Badge("Indisponible", BadgeTone.Danger),
        LlmStatus.Rejected => new Badge("Rejeté", BadgeTone.Danger),
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Statut LLM sans badge."),
    };
}
