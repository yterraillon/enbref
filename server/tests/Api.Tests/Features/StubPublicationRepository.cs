using EnBref.Infrastructure.Publication;

namespace EnBref.Api.Tests.Features;

/// <summary>Dépôt de publication qui renvoie un résultat choisi, garde les artefacts publiés et relit des artefacts choisis.</summary>
internal sealed class StubPublicationRepository(PublicationResult result) : IPublicationRepository
{
    public List<(string Artifact, string Content)> Publications { get; } = [];

    /// <summary>Contenu par artefact ; un artefact absent d'ici est absent du dépôt.</summary>
    public Dictionary<string, string> Artifacts { get; } = [];

    /// <summary>Résultat de publication par artefact, prioritaire sur le résultat commun.</summary>
    public Dictionary<string, PublicationResult> Results { get; } = [];

    /// <summary>Lecture en échec par artefact, hors absence (réseau, GitHub).</summary>
    public Dictionary<string, string> ReadErrors { get; } = [];

    public static StubPublicationRepository Succeeding() => new(new PublicationResult(new Uri("https://github.test/commit/1"), Error: null));

    public Task<PublicationResult> PublishAsync(string artifact, string content, string message, CancellationToken cancellationToken)
    {
        Publications.Add((artifact, content));
        return Task.FromResult(Results.GetValueOrDefault(artifact, result));
    }

    public Task<ArtifactReadResult> ReadAsync(string artifact, CancellationToken cancellationToken) =>
        Task.FromResult(ReadErrors.TryGetValue(artifact, out var error)
            ? new ArtifactReadResult(Content: null, error)
            : Artifacts.TryGetValue(artifact, out var content)
                ? new ArtifactReadResult(content, Error: null)
                : new ArtifactReadResult(Content: null, $"Artefact absent : {artifact}.", IsMissing: true));
}
