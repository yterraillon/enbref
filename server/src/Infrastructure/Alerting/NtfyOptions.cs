namespace EnBref.Infrastructure.Alerting;

/// <summary>
/// Section <c>Ntfy</c> : <c>Ntfy__Topic</c> et <c>Ntfy__Token</c> en variables d'environnement en
/// production. Sans topic, aucune alerte ne part.
/// </summary>
public sealed class NtfyOptions
{
    public const string SectionName = "Ntfy";

    public string Url { get; set; } = "https://ntfy.sh";

    public string Topic { get; set; } = "";

    /// <summary>Jeton d'accès ; vide pour un topic public.</summary>
    public string Token { get; set; } = "";
}
