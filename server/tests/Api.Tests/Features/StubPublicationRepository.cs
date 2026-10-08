using EnBref.Infrastructure.Publication;

namespace EnBref.Api.Tests.Features;

/// <summary>Publieur qui renvoie un résultat choisi et garde les artefacts publiés.</summary>
internal sealed class StubPublicationRepository(PublicationResult result) : IPublicationRepository
{
    public List<(string Artifact, string Content)> Publications { get; } = [];

    public static StubPublicationRepository Succeeding() => new(new PublicationResult(new Uri("https://github.test/commit/1"), Error: null));

    public Task<PublicationResult> PublishAsync(string artifact, string content, string message, CancellationToken cancellationToken)
    {
        Publications.Add((artifact, content));
        return Task.FromResult(result);
    }
}
