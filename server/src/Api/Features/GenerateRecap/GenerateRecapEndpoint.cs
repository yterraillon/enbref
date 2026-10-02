namespace EnBref.Api.Features.GenerateRecap;

public static class GenerateRecapEndpoint
{
    public static IServiceCollection AddGenerateRecap(this IServiceCollection services)
    {
        return services.AddScoped<GenerateRecapHandler>();
    }

    // POST uniquement : une génération consomme des crédits, elle ne part que sur une intention explicite.
    public static IEndpointRouteBuilder MapGenerateRecap(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/recaps/generations", async (
                GenerateRecapCommand command,
                GenerateRecapHandler handler,
                CancellationToken cancellationToken) =>
            {
                await handler.HandleAsync(command, cancellationToken);
                return Results.Ok();
            })
            .WithName("GenerateRecap")
            .WithTags("Génération");

        return endpoints;
    }
}
