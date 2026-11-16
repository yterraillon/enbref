using EnBref.Api.Features.ReadPublishedRecap;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Alerting;

namespace EnBref.Api.Features.CheckPublication;

/// <param name="IsSkipped">Génération quotidienne arrêtée : rien n'a été vérifié.</param>
/// <param name="Problem">Null si le récap du jour attendu est publié.</param>
public sealed record CheckPublicationResult(bool IsSkipped, string? Problem)
{
    public bool IsSuccessful => !IsSkipped && Problem is null;
}

/// <summary>
/// Vérification de publication : le récap du jour publié doit porter la date de la génération
/// quotidienne. Sinon, alerte. Rien n'est vérifié quand la génération quotidienne est arrêtée.
/// </summary>
public sealed class CheckPublicationHandler(
    ReadPublishedRecapHandler readPublishedRecap,
    DailyGenerationSettingsStore settings,
    IAlertSender alertSender,
    ILogger<CheckPublicationHandler> logger)
{
    private const string Missing = "Récap du jour absent";
    private const string Unchecked = "Récap du jour non vérifié";

    public async Task<CheckPublicationResult> HandleAsync(DateOnly expectedDate, CancellationToken cancellationToken)
    {
        if (!settings.Current.IsStarted)
        {
            logger.LogInformation("Génération quotidienne arrêtée : vérification de publication sautée.");
            return new CheckPublicationResult(IsSkipped: true, Problem: null);
        }

        var read = await readPublishedRecap.HandleAsync(RecapType.Daily, cancellationToken);
        // Une lecture en échec (réseau, JSON) ne prouve pas l'absence : titre distinct.
        var (title, problem) = read switch
        {
            { IsMissing: true } => (Missing, $"{read.Artifact} est absent du dépôt de publication."),
            { Recap: null } => (Unchecked, $"{read.Artifact} est illisible : {read.Error}"),
            { Recap.Date: var date } when date != expectedDate =>
                (Missing, $"{read.Artifact} est daté du {date:dd/MM/yyyy}, le récap du {expectedDate:dd/MM/yyyy} n'a pas été publié."),
            _ => (Missing, null),
        };

        if (problem is null)
        {
            logger.LogInformation("Vérification de publication : récap du {Date:yyyy-MM-dd} publié.", expectedDate);
            return new CheckPublicationResult(IsSkipped: false, Problem: null);
        }

        logger.LogError("Vérification de publication en échec : {Problem}", problem);
        await alertSender.SendAsync(new Alert(title, problem), cancellationToken);
        return new CheckPublicationResult(IsSkipped: false, problem);
    }
}
