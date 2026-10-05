using EnBref.Api.Features.CollectHeadlines;

namespace EnBref.Api.Features.GenerateRecap;

/// <param name="Recap">Null pour un récap de test, qui n'appelle pas le LLM, ou en cas d'échec.</param>
/// <param name="Error">Cause de l'échec de la génération, après une collecte réussie.</param>
public sealed record GenerateRecapResult(CollectionResult Collection, Recap? Recap, string? Error)
{
    public bool IsSuccessful => Collection.IsSuccessful && Error is null;
}
