using System.Reflection;

namespace EnBref.Api;

/// <summary>Version CalVer de l'image (ADR-004), injectée au build ; « dev » hors image.</summary>
public static class ServerVersion
{
    public static string Current { get; } = Read();

    public static DateTimeOffset StartedAt { get; } = DateTimeOffset.Now;

    private static string Read()
    {
        var version = typeof(ServerVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        // Le SDK suffixe le SHA du commit (« +abc123 ») : seul le CalVer nous intéresse.
        var calVer = version?.Split('+')[0];
        return string.IsNullOrEmpty(calVer) || calVer == "1.0.0" ? "dev" : calVer;
    }
}
