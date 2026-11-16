using System.Text.Json;
using Microsoft.Extensions.Options;

namespace EnBref.Api.Shared;

/// <summary>
/// Réglages de la génération quotidienne, persistés dans <c>settings.json</c> du répertoire des
/// données : un arrêt survit au redémarrage. Absent ou illisible, le fichier cède la place aux
/// réglages par défaut.
/// </summary>
public sealed class DailyGenerationSettingsStore
{
    private const string FileName = "settings.json";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly string path;
    private readonly Lock gate = new();
    private readonly ILogger<DailyGenerationSettingsStore> logger;
    private DailyGenerationSettings current;

    public DailyGenerationSettingsStore(IOptions<DataOptions> options, ILogger<DailyGenerationSettingsStore> logger)
    {
        this.logger = logger;
        path = Path.Combine(options.Value.Directory, FileName);
        current = Load();
    }

    public DailyGenerationSettings Current => current;

    /// <summary>Levé après chaque modification enregistrée, avec les nouveaux réglages.</summary>
    public event Action<DailyGenerationSettings>? Changed;

    // Les trois modifications renvoient null si elles sont enregistrées, sinon la cause, en français :
    // un /data en lecture seule ne doit pas faire tomber le back-office.

    public string? Start() => Update(settings => settings with { IsStarted = true });

    public string? Stop() => Update(settings => settings with { IsStarted = false });

    public string? SetTime(TimeOnly time) => Update(settings => settings with { Time = new TimeOnly(time.Hour, time.Minute) });

    private string? Update(Func<DailyGenerationSettings, DailyGenerationSettings> change)
    {
        DailyGenerationSettings updated;
        lock (gate)
        {
            updated = change(current);
            if (updated == current)
            {
                return null;
            }

            try
            {
                Save(updated);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                logger.LogError(exception, "Réglages non enregistrés dans {Path}.", path);
                return $"Réglages non enregistrés dans {path} : {exception.Message}";
            }

            current = updated;
        }

        logger.LogInformation("Génération quotidienne {State}, heure de génération {Time}.",
            updated.IsStarted ? "démarrée" : "arrêtée", updated.Time);
        Changed?.Invoke(updated);
        return null;
    }

    private DailyGenerationSettings Load()
    {
        if (!File.Exists(path))
        {
            logger.LogInformation("Pas de réglages dans {Path} : génération quotidienne arrêtée, heure de génération {Time}.", path, DailyGenerationSettings.Default.Time);
            return DailyGenerationSettings.Default;
        }

        try
        {
            return JsonSerializer.Deserialize<DailyGenerationSettings>(File.ReadAllText(path), JsonOptions)
                ?? DailyGenerationSettings.Default;
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(exception, "Réglages illisibles dans {Path} : réglages par défaut.", path);
            return DailyGenerationSettings.Default;
        }
    }

    // Fichier temporaire puis remplacement : un arrêt brutal ne laisse jamais un settings.json tronqué.
    private void Save(DailyGenerationSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var temporary = path + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(temporary, path, overwrite: true);
    }
}
