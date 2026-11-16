using EnBref.Api.Shared;

namespace EnBref.Api.Features.GenerateDailyRecap;

public static class GenerateDailyRecapSlice
{
    // Pas d'endpoint : la génération quotidienne part du job quotidien, à l'heure de génération.
    public static IServiceCollection AddGenerateDailyRecap(this IServiceCollection services)
    {
        services.AddSingleton(DailyRecapRetry.Default);
        services.AddScoped<GenerateDailyRecapHandler>();
        return services.AddHostedService<DailyRecapJob>();
    }
}

/// <summary>Job quotidien : déclenche la génération quotidienne à l'heure de génération.</summary>
public sealed class DailyRecapJob(
    IServiceScopeFactory scopes,
    DailyGenerationSettingsStore settings,
    TimeProvider timeProvider,
    ILogger<DailyRecapJob> logger) : DailyJob(settings, timeProvider, logger)
{
    protected override TimeSpan Offset => TimeSpan.Zero;

    protected override string Name => "Job quotidien";

    protected override async Task RunAsync(DateOnly generationDate, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<GenerateDailyRecapHandler>().HandleAsync(cancellationToken);
    }
}
