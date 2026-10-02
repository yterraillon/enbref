using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnBref.Infrastructure;

public static class DependencyInjection
{
    // Le lecteur de flux, l'agent de génération et le publieur s'enregistreront ici à partir de l'étape 2.
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }
}
