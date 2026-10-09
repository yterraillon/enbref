namespace EnBref.Infrastructure.Publication;

/// <param name="Content">Contenu brut de l'artefact.</param>
/// <param name="Error">Cause de l'échec de la lecture, artefact absent compris.</param>
/// <param name="IsMissing">L'artefact n'existe pas dans le dépôt (404) ; Error est alors renseigné.</param>
public sealed record ArtifactReadResult(string? Content, string? Error, bool IsMissing = false)
{
    public bool IsSuccessful => Error is null;
}
