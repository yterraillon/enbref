using Application;
using Infrastructure.Databases;
using Infrastructure.Notifications;
using Infrastructure.RssReader;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureBlocks(this IServiceCollection services, bool isUsingDocker = false)
    {
        if (isUsingDocker)
        {
            services.AddSingleton<IDbContext, DockerDbContext>();
        }
        else
        {
            services.AddSingleton<IDbContext, LocalDbContext>();
        }

        services.AddTransient<IRssReader, RssReaderService>();
        services.AddHttpClient<INotificationService, Ntfy>("ntfy-client", client =>
        {
            client.BaseAddress = new Uri("https://ntfy.checquy.ovh/myfanwy");
        });
    }
}