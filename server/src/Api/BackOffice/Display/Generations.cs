using EnBref.Api.Features.GenerateRecap;
using EnBref.Api.Shared;

namespace EnBref.Api.BackOffice.Display;

/// <summary>Déclenchement d'une génération depuis le back-office.</summary>
public static class Generations
{
    /// <summary>
    /// Une génération qui consomme des crédits et remplace un récap relu (jour, démo) se confirme ;
    /// le récap de test, sans LLM et isolé sur test.json, part au premier clic.
    /// </summary>
    public static bool RequiresConfirmation(RecapType type) => type != RecapType.Test;

    /// <summary>« Récap de démo généré et publié. » ou « Erreur lors de la collecte : … ».</summary>
    public static string Feedback(RecapType type, GenerateRecapResult result) =>
        result.Failure is { } failure
            ? $"Erreur lors de la {failure.Step.Label()} : {failure.Message}"
            : $"{type.Label()} généré et publié.";
}
