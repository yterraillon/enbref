using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnBref.Api.Features.PublishRecapFeed;

public static class PublishRecapFeedSlice
{
    // Pas d'endpoint : le flux du récap se publie à la suite du récap du jour, dans GenerateRecap.
    public static IServiceCollection AddPublishRecapFeed(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        return services.AddScoped<PublishRecapFeedHandler>();
    }
}
