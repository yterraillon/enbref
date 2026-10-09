using EnBref.Api.Features.ReadPublishedRecap;
using EnBref.Api.Shared;

namespace EnBref.Api.BackOffice.Display;

/// <summary>
/// « Dernier récap généré » de l'accueil : sans historique, c'est le plus récent des récaps publiés
/// lisibles, et à date égale le récap du jour, puis de démo, puis de test.
/// </summary>
public static class LatestRecap
{
    public static RecapType? Pick(IReadOnlyDictionary<RecapType, ReadPublishedRecapResult> reads) =>
        reads
            .Where(read => read.Value.Recap is not null)
            .OrderByDescending(read => read.Value.Recap!.Date)
            .ThenBy(read => read.Key)
            .Select(read => (RecapType?)read.Key)
            .FirstOrDefault();
}
