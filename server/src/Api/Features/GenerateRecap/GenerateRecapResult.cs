using EnBref.Api.Features.CollectHeadlines;
using EnBref.Infrastructure.Publication;

namespace EnBref.Api.Features.GenerateRecap;

/// <param name="Recap">Null en cas d'échec de la collecte ou de la génération.</param>
/// <param name="Error">Cause de l'échec de la génération, après une collecte réussie.</param>
/// <param name="Artifact">Artefact visé ; null tant que la publication n'est pas tentée.</param>
/// <param name="Publication">Null si la publication n'est pas demandée ou pas atteinte.</param>
public sealed record GenerateRecapResult(
    CollectionResult Collection,
    Recap? Recap,
    string? Error,
    string? Artifact = null,
    PublicationResult? Publication = null)
{
    public bool IsSuccessful => Collection.IsSuccessful && Error is null && Publication?.IsSuccessful != false;
}
