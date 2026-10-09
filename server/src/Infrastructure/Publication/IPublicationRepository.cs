namespace EnBref.Infrastructure.Publication;

/// <summary>Dépôt de publication : écrit et relit un artefact, sans rien savoir du récap qu'il contient.</summary>
public interface IPublicationRepository
{
    /// <summary>Constate le résultat de la publication sans jamais lever, hors annulation.</summary>
    Task<PublicationResult> PublishAsync(string artifact, string content, string message, CancellationToken cancellationToken);

    /// <summary>Lit l'artefact à la source, sans passer par le CDN. Constate sans jamais lever, hors annulation.</summary>
    Task<ArtifactReadResult> ReadAsync(string artifact, CancellationToken cancellationToken);
}
