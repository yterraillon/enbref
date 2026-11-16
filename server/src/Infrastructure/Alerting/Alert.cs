namespace EnBref.Infrastructure.Alerting;

/// <summary>Alerte envoyée sur ntfy — voir docs/ubiquitous-language.md.</summary>
/// <param name="Title">Titre court, affiché en tête de l'alerte.</param>
/// <param name="Message">Cause lisible, en français.</param>
public sealed record Alert(string Title, string Message);

/// <param name="Error">Null si l'alerte est partie.</param>
public sealed record AlertResult(string? Error)
{
    public bool IsSent => Error is null;
}
