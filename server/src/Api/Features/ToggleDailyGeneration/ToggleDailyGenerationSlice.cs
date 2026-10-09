namespace EnBref.Api.Features.ToggleDailyGeneration;

public static class ToggleDailyGenerationSlice
{
    // Pas d'endpoint : la génération quotidienne ne se bascule que depuis le back-office.
    public static IServiceCollection AddToggleDailyGeneration(this IServiceCollection services) =>
        services.AddSingleton<DailyGenerationSwitch>();
}
