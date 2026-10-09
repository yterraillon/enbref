namespace EnBref.Api.Features.ReadPublishedRecap;

public static class ReadPublishedRecapSlice
{
    // Pas d'endpoint : le récap publié n'est lu que par le back-office, en mémoire.
    public static IServiceCollection AddReadPublishedRecap(this IServiceCollection services) =>
        services.AddScoped<ReadPublishedRecapHandler>();
}
