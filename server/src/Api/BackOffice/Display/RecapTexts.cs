using EnBref.Api.Features.ReadPublishedRecap;
using EnBref.Api.Shared;

namespace EnBref.Api.BackOffice.Display;

/// <summary>Textes des cartes de récap de l'accueil et de la page Récaps.</summary>
public static class RecapTexts
{
    /// <summary>État du récap du jour, sur la carte de l'accueil.</summary>
    public static string Daily(ReadPublishedRecapResult result) => result switch
    {
        { Recap: { } recap, IsFromToday: true } => $"Publié aujourd’hui · {RecapCounts.Describe(recap)}.",
        { Recap: { } recap } => $"Le récap publié date du {FrenchDates.Long(recap.Date)} : il ne date pas d’aujourd’hui.",
        { IsMissing: true } => $"Aucun récap du jour n’a encore été publié : le fichier {result.Artifact} n’existe pas. "
            + "Générez un récap de démo pour vérifier le rendu, ou publiez un récap de test.",
        _ => result.Error ?? "",
    };

    /// <summary>« Vendredi 9 octobre 2026 · demo.json · 5 catégories · 10 brèves ».</summary>
    public static string Meta(ReadPublishedRecapResult result) => result switch
    {
        { Recap: { } recap } => $"{FrenchDates.Day(recap.Date)} {recap.Date.Year} · {result.Artifact} · {RecapCounts.Describe(recap)}",
        { IsMissing: true } => $"{result.Artifact} · pas encore généré",
        _ => $"{result.Artifact} · illisible",
    };

    public static string EmptyTitle(RecapType type) => type switch
    {
        RecapType.Daily => "Pas encore de récap du jour",
        RecapType.Demo => "Pas encore de récap de démo",
        RecapType.Test => "Aucun récap de test",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Type de récap sans texte."),
    };

    public static string EmptyBody(RecapType type) => type switch
    {
        RecapType.Daily => $"Le fichier {type.Artifact()} n’existe pas. Régénérez le récap pour qu’il apparaisse ici et dans l’app.",
        RecapType.Demo => $"Le fichier {type.Artifact()} n’existe pas. Régénérez le récap de démo pour la revue App Store et le test de chargement.",
        RecapType.Test => "Régénérez le récap de test pour vérifier la chaîne de publication sans toucher au récap du jour.",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Type de récap sans texte."),
    };
}
