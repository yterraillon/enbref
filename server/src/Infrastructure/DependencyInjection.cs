using EnBref.Infrastructure.Collection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnBref.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FeedOptions>(configuration.GetSection(FeedOptions.SectionName));

        services.AddHttpClient<RssFeedReader>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; EnBref/1.0)");
        });
        services.AddKeyedTransient<IFeedReader>(FeedReaderKeys.Rss, (provider, _) => provider.GetRequiredService<RssFeedReader>());
        services.AddKeyedSingleton<IFeedReader, FakeFeedReader>(FeedReaderKeys.Fake);

        return services;
    }
}
