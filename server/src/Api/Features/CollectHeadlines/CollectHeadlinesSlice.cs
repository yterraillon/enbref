namespace EnBref.Api.Features.CollectHeadlines;

public static class CollectHeadlinesSlice
{
    // Pas d'endpoint : la collecte est appelée en mémoire par la génération et le back-office.
    public static IServiceCollection AddCollectHeadlines(this IServiceCollection services) =>
        services.AddScoped<CollectHeadlinesHandler>();
}
