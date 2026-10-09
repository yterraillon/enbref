namespace EnBref.Api.BackOffice.Display;

/// <summary>Ton d'un StatusBadge du design system ; toujours accompagné d'un libellé.</summary>
public enum BadgeTone
{
    Neutral,
    Success,
    Warning,
    Danger,
}

/// <summary>Badge prêt à afficher : libellé et ton.</summary>
public sealed record Badge(string Label, BadgeTone Tone);
