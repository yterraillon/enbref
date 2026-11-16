using System.Xml;
using System.Xml.Linq;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Alerting;
using EnBref.Infrastructure.Publication;

namespace EnBref.Api.Features.PublishRecapFeed;

/// <summary>
/// Ajoute le récap du jour au flux du récap et le republie (ADR-012). Un échec lève une alerte mais
/// ne fait jamais échouer la génération : latest.json reste l'artefact de référence, et une nouvelle
/// tentative coûterait une génération LLM.
/// </summary>
public sealed class PublishRecapFeedHandler(
    IPublicationRepository publicationRepository,
    IAlertSender alertSender,
    TimeProvider timeProvider,
    ILogger<PublishRecapFeedHandler> logger)
{
    private const string AlertTitle = "Flux du récap non publié";

    public async Task<PublicationResult> HandleAsync(Recap recap, CancellationToken cancellationToken)
    {
        var read = await publicationRepository.ReadAsync(RecapFeed.Artifact, cancellationToken);
        if (!read.IsSuccessful && !read.IsMissing)
        {
            // Republier sans relire effacerait l'historique du flux sur une simple erreur réseau.
            return await FailAsync(recap, $"{RecapFeed.Artifact} n'a pas pu être relu : {read.Error}", cancellationToken);
        }

        var publishedAt = timeProvider.GetUtcNow();
        var feed = RecapFeed.Merge(Existing(read.Content), RecapFeed.Item(recap, publishedAt), publishedAt);
        var publication = await publicationRepository.PublishAsync(RecapFeed.Artifact, feed,
            $"Publication du flux du récap du {recap.Date:yyyy-MM-dd}", cancellationToken);

        return publication.IsSuccessful
            ? publication
            : await FailAsync(recap, $"Publication sur {RecapFeed.Artifact} impossible : {publication.Error}", cancellationToken);
    }

    private XDocument? Existing(string? content)
    {
        if (content is null)
        {
            return null;
        }

        try
        {
            return RecapFeed.Parse(content);
        }
        catch (XmlException exception)
        {
            // Un flux illisible n'a plus d'historique à sauver : il repart du seul item du jour.
            logger.LogWarning("{Artifact} illisible, le flux repart de zéro : {Error}", RecapFeed.Artifact, exception.Message);
            return null;
        }
    }

    private async Task<PublicationResult> FailAsync(Recap recap, string error, CancellationToken cancellationToken)
    {
        logger.LogError("Flux du récap du {Date:yyyy-MM-dd} non publié : {Error}", recap.Date, error);
        await alertSender.SendAsync(new Alert(AlertTitle, error), cancellationToken);
        return new PublicationResult(CommitUrl: null, error);
    }
}
