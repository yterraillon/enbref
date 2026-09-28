using EnBref.Application;
using EnBref.Infrastructure;
using Settings = Infrastructure.Notifications.Settings;

namespace Api.App;

public static class DependencyInjection
{
    public static void LoadModules(this IServiceCollection services)
    {
        services.AddEnBrefApplication();
        services.AddEnBrefInfrastructure();
    }

    public static void LoadConfigurations(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddSingleton<global::EnBref.Infrastructure.Settings>(_ => new global::EnBref.Infrastructure.Settings
        {
            OpenAiApiKey = builder.Environment.IsDevelopment() ?
                builder.Configuration["OpenAiApiKey"] :
                Environment.GetEnvironmentVariable("OpenAiApiKey"),
            GithubToken = builder.Environment.IsDevelopment() ?
                builder.Configuration["GithubToken"] :
                Environment.GetEnvironmentVariable("GithubToken"),
        });

        services.AddSingleton<Settings>(_ => new Settings
        {
            NtfyToken = builder.Environment.IsDevelopment() ?
                builder.Configuration["NtfyToken"] :
                Environment.GetEnvironmentVariable("NtfyToken")
        });
    }
}
