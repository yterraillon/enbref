using EnBref.Api.Shared;

namespace EnBref.Api.Features.CheckPublication;

public static class CheckPublicationSlice
{
    // Pas d'endpoint : la vérification de publication part de son job, une heure après l'heure de génération.
    public static IServiceCollection AddCheckPublication(this IServiceCollection services)
    {
        services.AddScoped<CheckPublicationHandler>();
        return services.AddHostedService<PublicationCheckJob>();
    }
}

/// <summary>Déclenche la vérification de publication une heure après l'heure de génération.</summary>
public sealed class PublicationCheckJob(
    IServiceScopeFactory scopes,
    DailyGenerationSettingsStore settings,
    TimeProvider timeProvider,
    ILogger<PublicationCheckJob> logger) : DailyJob(settings, timeProvider, logger)
{
    public static readonly TimeSpan Delay = TimeSpan.FromHours(1);

    protected override TimeSpan Offset => Delay;

    protected override string Name => "Vérification de publication";

    protected override async Task RunAsync(DateOnly generationDate, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<CheckPublicationHandler>().HandleAsync(generationDate, cancellationToken);
    }
}
