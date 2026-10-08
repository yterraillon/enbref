using Anthropic;
using EnBref.Infrastructure.Collection;
using EnBref.Infrastructure.Llm;
using EnBref.Infrastructure.Publication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        services.Configure<AnthropicOptions>(configuration.GetSection(AnthropicOptions.SectionName));
        services.AddSingleton(provider => new AnthropicClient
        {
            ApiKey = provider.GetRequiredService<IOptions<AnthropicOptions>>().Value.ApiKey,
        });
        services.AddSingleton<ILlmClient, AnthropicLlmClient>();

        // Clé à plat, comme dans les stacks locale et de production et dans le secret de CI.
        services.AddOptions<GithubOptions>().Configure(options => options.Token = configuration["GithubToken"] ?? "");
        services.AddHttpClient<IPublicationRepository, GithubPublicationRepository>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EnBref/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        });

        return services;
    }
}
