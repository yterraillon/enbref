namespace EnBref.Infrastructure.Publication;

/// <param name="CommitUrl">Commit qui porte l'artefact publié.</param>
/// <param name="Error">Cause de l'échec de la publication.</param>
public sealed record PublicationResult(Uri? CommitUrl, string? Error)
{
    public bool IsSuccessful => Error is null;
}
