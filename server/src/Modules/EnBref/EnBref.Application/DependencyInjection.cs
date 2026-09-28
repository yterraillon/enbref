using Microsoft.Extensions.DependencyInjection;

namespace EnBref.Application;

public static class DependencyInjection
{
    public static void AddEnBrefApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssemblyContaining<EnBref>());
    }
}