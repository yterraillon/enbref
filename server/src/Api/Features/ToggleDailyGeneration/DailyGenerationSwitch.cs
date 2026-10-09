namespace EnBref.Api.Features.ToggleDailyGeneration;

/// <summary>
/// Génération quotidienne démarrée ou arrêtée — voir docs/ubiquitous-language.md. Lue par le futur job
/// quotidien ; sans effet tant qu'il n'existe pas. En mémoire : repart démarrée au redémarrage.
/// </summary>
public sealed class DailyGenerationSwitch
{
    private volatile bool isStarted = true;

    public bool IsStarted => isStarted;

    public void Start() => isStarted = true;

    public void Stop() => isStarted = false;
}
