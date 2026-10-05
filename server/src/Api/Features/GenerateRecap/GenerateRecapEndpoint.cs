using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnBref.Api.Features.GenerateRecap;

public static class GenerateRecapEndpoint
{
    public static IServiceCollection AddGenerateRecap(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<GenerationAgent>();
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
                var result = await handler.HandleAsync(command, cancellationToken);
                var response = GenerateRecapResponse.From(result);

                return result.IsSuccessful
                    ? Results.Ok(response)
                    : Results.Problem(result.Error is null ? "Aucun titre collecté." : $"Génération impossible : {result.Error}",
                        statusCode: StatusCodes.Status502BadGateway,
                        extensions: new Dictionary<string, object?> { ["feeds"] = response.Feeds });
            })
            .WithName("GenerateRecap")
            .WithTags("Génération");

        return endpoints;
    }
}

public sealed record GenerateRecapResponse(
    int HeadlineCount,
    IReadOnlyList<GenerateRecapResponse.FeedSummary> Feeds,
    Recap? Recap)
{
    public sealed record FeedSummary(string Source, string Status, int HeadlineCount, string? Error);

    public static GenerateRecapResponse From(GenerateRecapResult result) => new(
        result.Collection.Headlines.Count,
        result.Collection.Feeds
            .Select(feed => new FeedSummary(feed.Feed.Source, feed.Status.ToString(), feed.Headlines.Count, feed.Error))
            .ToList(),
        result.Recap);
}
