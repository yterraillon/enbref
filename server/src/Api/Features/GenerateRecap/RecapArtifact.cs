using EnBref.Api.Shared;

namespace EnBref.Api.Features.GenerateRecap;

/// <summary>
/// Seul endroit qui nomme un artefact publié. L'artefact découle du type de récap, jamais d'un
/// paramètre d'appel ni de la configuration : un récap de test n'atteint que test.json (ADR-007).
/// </summary>
public static class RecapArtifact
{
    public static string For(RecapType type) => type switch
    {
        RecapType.Daily => "latest.json",
        RecapType.Demo => "demo.json",
        RecapType.Test => "test.json",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Type de récap sans artefact."),
    };
}
