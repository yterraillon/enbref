namespace EnBref.Api.Shared;

/// <summary>
/// Génération quotidienne démarrée ou arrêtée, et son heure de génération — voir
/// docs/ubiquitous-language.md.
/// </summary>
/// <param name="IsStarted">Arrêtée : le job quotidien ne produit aucun récap du jour.</param>
/// <param name="Time">Heure de génération, heure locale du serveur (Europe/Paris).</param>
public sealed record DailyGenerationSettings(bool IsStarted, TimeOnly Time)
{
    /// <summary>
    /// Arrêtée par défaut : une stack neuve (locale, CI, premier déploiement) ne génère rien, et ne
    /// consomme aucun crédit, tant qu'on ne la démarre pas depuis le back-office.
    /// </summary>
    public static DailyGenerationSettings Default { get; } = new(IsStarted: false, new TimeOnly(17, 0));
}
