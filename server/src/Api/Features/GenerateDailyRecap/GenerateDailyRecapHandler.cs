using EnBref.Api.Features.GenerateRecap;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Alerting;

namespace EnBref.Api.Features.GenerateDailyRecap;

/// <summary>Tentatives de la génération quotidienne avant alerte — voir docs/ubiquitous-language.md.</summary>
public sealed record DailyRecapRetry(int Attempts, TimeSpan Delay)
{
    public static DailyRecapRetry Default { get; } = new(3, TimeSpan.FromMinutes(10));
}

/// <param name="IsSkipped">Génération quotidienne arrêtée avant la première tentative ou entre deux.</param>
/// <param name="Attempts">Tentatives faites.</param>
/// <param name="Last">Résultat de la dernière tentative ; null si rien n'a été tenté ou si elle a levé une exception.</param>
public sealed record GenerateDailyRecapResult(bool IsSkipped, int Attempts, GenerateRecapResult? Last)
{
    public bool IsSuccessful => Last?.IsSuccessful ?? false;
}

/// <summary>
/// Génération quotidienne : génère et publie le récap du jour si elle est démarrée, retente, puis
/// lève une alerte quand toutes les tentatives ont échoué.
/// </summary>
public sealed class GenerateDailyRecapHandler(
    GenerateRecapHandler generateRecap,
    DailyGenerationSettingsStore settings,
    IAlertSender alertSender,
    DailyRecapRetry retry,
    TimeProvider timeProvider,
    ILogger<GenerateDailyRecapHandler> logger)
{
    /// <param name="generationDate">Date du récap du jour, gardée par toutes les tentatives.</param>
    public async Task<GenerateDailyRecapResult> HandleAsync(DateOnly generationDate, CancellationToken cancellationToken)
    {
        GenerateRecapResult? result = null;
        var failure = "";
        for (var attempt = 1; attempt <= retry.Attempts; attempt++)
        {
            // Relu à chaque tentative : un arrêt pendant l'attente empêche les suivantes.
            if (!settings.Current.IsStarted)
            {
                logger.LogInformation("Génération quotidienne arrêtée : aucun récap du jour.");
                return new GenerateDailyRecapResult(IsSkipped: true, attempt - 1, result);
            }

            try
            {
                result = await generateRecap.HandleAsync(
                    new GenerateRecapCommand(RecapType.Daily, Publish: true, generationDate), cancellationToken);
                if (result.IsSuccessful)
                {
                    logger.LogInformation("Récap du jour publié à la tentative {Attempt} : {CommitUrl}.", attempt, result.Publication?.CommitUrl);
                    return new GenerateDailyRecapResult(IsSkipped: false, attempt, result);
                }

                failure = $"Erreur lors de la {result.Failure!.Step.Label()}, après {retry.Attempts} tentatives : {result.Failure.Message}";
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
            {
                // Une exception inattendue (SDK, réseau) compte comme une tentative en échec.
                logger.LogError(exception, "Génération quotidienne en échec, tentative {Attempt}.", attempt);
                result = null;
                failure = $"Erreur inattendue, après {retry.Attempts} tentatives : {exception.Message}";
            }

            if (attempt < retry.Attempts)
            {
                logger.LogWarning("Génération quotidienne en échec, tentative {Attempt}/{Attempts} : nouvel essai dans {Delay}.",
                    attempt, retry.Attempts, retry.Delay);
                await Task.Delay(retry.Delay, timeProvider, cancellationToken);
            }
        }

        await alertSender.SendAsync(new Alert("Récap du jour non publié", failure), cancellationToken);
        return new GenerateDailyRecapResult(IsSkipped: false, retry.Attempts, result);
    }
}
