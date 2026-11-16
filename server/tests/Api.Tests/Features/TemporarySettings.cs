using EnBref.Api.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EnBref.Api.Tests.Features;

/// <summary>Réglages de la génération quotidienne dans un répertoire temporaire, supprimé au <see cref="Dispose"/>.</summary>
internal sealed class TemporarySettings : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "enbref-tests", Guid.NewGuid().ToString("N"));

    public DailyGenerationSettingsStore Store() =>
        new(Options.Create(new DataOptions { Directory = directory }), NullLogger<DailyGenerationSettingsStore>.Instance);

    public DailyGenerationSettingsStore Store(bool isStarted)
    {
        var store = Store();
        if (isStarted)
        {
            store.Start();
        }

        return store;
    }

    /// <summary>Store dont le répertoire des données est un fichier : toute écriture échoue.</summary>
    public DailyGenerationSettingsStore Unwritable()
    {
        Directory.CreateDirectory(directory);
        var blocked = Path.Combine(directory, "data");
        File.WriteAllText(blocked, "");
        return new(Options.Create(new DataOptions { Directory = blocked }), NullLogger<DailyGenerationSettingsStore>.Instance);
    }

    public void Write(string content)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "settings.json"), content);
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
