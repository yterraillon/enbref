using System.Text.Json;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Publication;

namespace EnBref.Api.Features.ReadPublishedRecap;

/// <param name="Artifact">Artefact lu, qui découle du type de récap.</param>
/// <param name="Recap">Null si l'artefact est illisible ou ne suit pas le contrat.</param>
/// <param name="Content">Contenu brut, conservé quand il ne suit pas le contrat.</param>
/// <param name="Error">Null si le récap publié a été lu.</param>
public sealed record ReadPublishedRecapResult(string Artifact, Recap? Recap, string? Content, string? Error)
{
    public bool IsSuccessful => Error is null;
}

/// <summary>Lit un récap publié à la source, sans passer par le CDN.</summary>
public sealed class ReadPublishedRecapHandler(IPublicationRepository publicationRepository)
{
    public async Task<ReadPublishedRecapResult> HandleAsync(RecapType type, CancellationToken cancellationToken)
    {
        var artifact = type.Artifact();
        var read = await publicationRepository.ReadAsync(artifact, cancellationToken);
        if (read.Content is not { } content)
        {
            return new ReadPublishedRecapResult(artifact, Recap: null, Content: null, read.Error);
        }

        try
        {
            return new ReadPublishedRecapResult(artifact, RecapContract.Deserialize(content), content, Error: null);
        }
        catch (JsonException exception)
        {
            return new ReadPublishedRecapResult(artifact, Recap: null, content, $"{artifact} ne suit pas le contrat : {exception.Message}");
        }
    }
}
