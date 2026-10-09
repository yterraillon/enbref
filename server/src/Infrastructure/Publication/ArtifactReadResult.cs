namespace EnBref.Infrastructure.Publication;

/// <param name="Content">Contenu brut de l'artefact.</param>
/// <param name="Error">Cause de l'échec de la lecture, artefact absent compris.</param>
public sealed record ArtifactReadResult(string? Content, string? Error)
{
    public bool IsSuccessful => Error is null;
}
