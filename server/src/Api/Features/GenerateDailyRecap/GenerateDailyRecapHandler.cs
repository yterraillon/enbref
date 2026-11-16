using EnBref.Api.Features.GenerateRecap;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Alerting;

namespace EnBref.Api.Features.GenerateDailyRecap;

/// <summary>Tentatives de la génération quotidienne avant alerte — voir docs/ubiquitous-language.md.</summary>
public sealed record DailyRecapRetry(int Attempts, TimeSpan Delay)
{
    public static DailyRecapRetry Default { get; } = new(3, TimeSpan.FromMinutes(10));
}

/// <param name="IsSkipped">Génération quotidienne arrêtée : rien n'a été tenté.</param>
/// <param name="Attempts">Tentatives faites.</param>
/// <param name="Last">Résultat de la dernière tentative ; null si rien n'a été tenté.</param>
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
    public async Task<GenerateDailyRecapResult> HandleAsync(CancellationToken cancellationToken)
    {
        if (!settings.Current.IsStarted)
        {
            logger.LogInformation("Génération quotidienne arrêtée : aucun récap du jour.");
            return new GenerateDailyRecapResult(IsSkipped: true, Attempts: 0, Last: null);
        }

        GenerateRecapResult result = null!;
        for (var attempt = 1; attempt <= retry.Attempts; attempt++)
        {
            result = await generateRecap.HandleAsync(new GenerateRecapCommand(RecapType.Daily, Publish: true), cancellationToken);
            if (result.IsSuccessful)
            {
                logger.LogInformation("Récap du jour publié à la tentative {Attempt} : {CommitUrl}.", attempt, result.Publication?.CommitUrl);
                return new GenerateDailyRecapResult(IsSkipped: false, attempt, result);
            }

            if (attempt < retry.Attempts)
            {
                logger.LogWarning("Génération quotidienne en échec, tentative {Attempt}/{Attempts} : nouvel essai dans {Delay}.",
                    attempt, retry.Attempts, retry.Delay);
                await Task.Delay(retry.Delay, timeProvider, cancellationToken);
            }
        }

        var failure = result.Failure!;
        await alertSender.SendAsync(new Alert(
            "Récap du jour non publié",
            $"Erreur lors de la {failure.Step.Label()}, après {retry.Attempts} tentatives : {failure.Message}"), cancellationToken);
        return new GenerateDailyRecapResult(IsSkipped: false, retry.Attempts, result);
    }
}
