namespace EnBref.Infrastructure.Collection;

/// <summary>Fausse source du récap de test : titres fixes, aucun appel réseau.</summary>
public sealed class FakeFeedReader : IFeedReader
{
    private static readonly string[] Headlines =
    [
        "Le gouvernement présente son projet de budget",
        "Sommet international sur le climat à Genève",
        "L'inflation recule pour le troisième mois consécutif",
        "Grève dans les transports en Île-de-France",
        "Une nouvelle puce promet de diviser par deux la consommation des data centers",
        "Le XV de France s'impose face à l'Irlande",
        "Le festival d'Avignon dévoile sa programmation",
    ];

    public Task<FeedResult> ReadAsync(Feed feed, CancellationToken cancellationToken) =>
        Task.FromResult(new FeedResult(feed, FeedStatus.Available, Headlines));
}
