namespace EnBref.Infrastructure.Publication;

/// <summary>Dépôt de publication : écrit un artefact, sans rien savoir du récap qu'il contient.</summary>
public interface IPublicationRepository
{
    /// <summary>Constate le résultat de la publication sans jamais lever, hors annulation.</summary>
    Task<PublicationResult> PublishAsync(string artifact, string content, string message, CancellationToken cancellationToken);
}
