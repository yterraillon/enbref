using System.Net.Http.Headers;
using System.Reflection;
using Application.AiAgents;
using EnBref.Application.Contracts;
using EnBref.Infrastructure.Databases;
using EnBref.Infrastructure.GithubCdn;
using EnBref.Infrastructure.OpenAiAgents.RecapBuilder;
using EnBref.Infrastructure.OpenAiAgents.RecapFormatter;
using EnBref.Infrastructure.RssReader;
using EnBref.Infrastructure.ScheduledJobs;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace EnBref.Infrastructure;

public static class DependencyInjection
{
    public static void AddEnBrefInfrastructure(this IServiceCollection services, bool isUsingDocker = false)
    {
        var openAiUrl = new Uri("https://api.openai.com/v1/chat/completions");
        var githubApiUrl = new Uri("https://api.github.com");
        var githubCdnUrl = new Uri("https://yterraillon.github.io");
        
        // services.AddSingleton<IObjectStorageReader<Recap>, AzureBlobStorageReader>();
        // services.AddSingleton<IObjectStorageWriter<Recap>, AzureBlobStorageWriter>();
        
        services.AddHttpClient<IObjectStorageWriter<Recap>, GithubCdnPublisher>("en-bref-cdn-publisher", client =>
        {
            client.BaseAddress = githubApiUrl;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; EnBref/1.0)");
            client.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        });
        
        services.AddHttpClient<IObjectStorageReader<Recap>, GithubCdnReader>("en-bref-cdn-reader", client =>
        {
            client.BaseAddress = githubCdnUrl;
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; EnBref/1.0)");
            client.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
        
        services.AddTransient<IRssReader, RssReaderService>();
        services.AddTransient<LocalStorageService>();

        if (isUsingDocker)
        {
            services.AddSingleton<ILocalStorageContext, DockerLocalStorageContext>();
        }
        else
        {
            services.AddSingleton<ILocalStorageContext, LocalStorageContext>();
        }
        
        services.AddHttpClient<IAiAgent<IEnumerable<string>, string>, RecapBuilderAgent>("recap-builder-agent", client =>
        {
            client.BaseAddress = openAiUrl;
        });
        
        services.AddHttpClient<IAiAgent<string, Recap>, RecapFormatterAgent>("recap-formatter-agent", client =>
        {
            client.BaseAddress = openAiUrl;
        });

        // Database & Metrics
        services.AddSingleton<EnBrefDbContext>();
        services.AddAutoMapper(configuration =>
        {
            configuration.AddMaps(Assembly.GetExecutingAssembly());
        });
        services.AddTransient<IRecapSectionMetricRepository, RecapSectionMetricRepository>();

        services.AddQuartz(q =>
        {
            q.AddJob<GenerateDailyRecapJob>(opts => opts.WithIdentity(GenerateDailyRecapJob.Key));
            q.AddTrigger(opts => opts
                .ForJob(GenerateDailyRecapJob.Key)
                .WithIdentity("EnBrefDailyRecap-trigger")
                .WithCronSchedule("0 0 17 * * ?")); //Run every day at 17:00
        });
        
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }
}