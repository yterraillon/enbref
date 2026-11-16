namespace EnBref.Api.Shared;

public static class DailyGenerationSettingsRegistration
{
    // Lu par le job quotidien, la vérification de publication et le back-office, qui seul le modifie.
    public static IServiceCollection AddDailyGenerationSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DataOptions>(configuration.GetSection(DataOptions.SectionName));
        return services.AddSingleton<DailyGenerationSettingsStore>();
    }
}
