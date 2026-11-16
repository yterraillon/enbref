namespace EnBref.Api.Shared;

/// <summary>
/// Boucle d'un job qui tourne chaque jour à l'heure de génération, plus un décalage : le job quotidien
/// et la vérification de publication. Elle attend la prochaine échéance, la recalcule dès que les
/// réglages changent, et ne décide rien : la règle métier est dans le handler appelé par
/// <see cref="RunAsync"/>.
/// </summary>
public abstract class DailyJob(DailyGenerationSettingsStore settings, TimeProvider timeProvider, ILogger logger) : BackgroundService
{
    /// <summary>Décalage par rapport à l'heure de génération.</summary>
    protected abstract TimeSpan Offset { get; }

    /// <summary>Nom du job dans les logs.</summary>
    protected abstract string Name { get; }

    /// <param name="generationDate">Date de la génération à laquelle cette exécution se rattache.</param>
    protected abstract Task RunAsync(DateOnly generationDate, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Jamais deux exécutions pour la même échéance, même si le minuteur se réveille en avance.
        var notBefore = DateTimeOffset.MinValue;

        while (!stoppingToken.IsCancellationRequested)
        {
            using var changed = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            void OnChanged(DailyGenerationSettings _)
            {
                try
                {
                    changed.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Changement arrivé pendant que la boucle passait à l'échéance suivante.
                }
            }

            settings.Changed += OnChanged;
            try
            {
                var now = timeProvider.GetUtcNow();
                var (due, generationDate) = DailySchedule.NextRun(
                    now > notBefore ? now : notBefore, settings.Current.Time, Offset, timeProvider.LocalTimeZone);
                logger.LogInformation("{Job} : prochaine exécution le {Due:yyyy-MM-dd HH:mm zzz}.", Name, due);

                var delay = due - now;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, timeProvider, changed.Token);
                }

                notBefore = due;
                await RunAsync(generationDate, stoppingToken);
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("{Job} : réglages modifiés, échéance recalculée.", Name);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(exception, "{Job} : exécution en échec.", Name);
            }
            finally
            {
                settings.Changed -= OnChanged;
            }
        }
    }
}
