using EnBref.Api.Shared;

namespace EnBref.Api.BackOffice.Display;

/// <summary>Libellés de la génération quotidienne : heure de génération, prochaine génération, bandeau.</summary>
public static class DailyGenerationTexts
{
    /// <summary>« 17 h », « 6 h 30 ».</summary>
    public static string Time(TimeOnly time) => time.Minute == 0 ? $"{time.Hour} h" : $"{time.Hour} h {time.Minute:00}";

    /// <summary>« Aujourd'hui à 17 h », « Demain à 6 h 30 » ; « Aucune » si la génération quotidienne est arrêtée.</summary>
    public static string NextGeneration(DailyGenerationSettings settings, DateTimeOffset now, TimeZoneInfo zone)
    {
        if (!settings.IsStarted)
        {
            return "Aucune, génération quotidienne arrêtée";
        }

        var next = DailySchedule.NextOccurrence(now, settings.Time, zone);
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
        var day = DateOnly.FromDateTime(next.DateTime);
        var label = day == today ? "Aujourd’hui" : day == today.AddDays(1) ? "Demain" : FrenchDates.Day(day);
        return $"{label} à {Time(TimeOnly.FromDateTime(next.DateTime))}";
    }

    /// <summary>Topic ntfy des alertes, ou « Non configuré » : aucune alerte ne part.</summary>
    public static string Topic(string? topic) => string.IsNullOrWhiteSpace(topic) ? "Non configuré" : topic;

    /// <summary>Bandeau de toutes les pages ; null tant que la génération quotidienne est démarrée.</summary>
    public static string? Banner(DailyGenerationSettings settings) => settings.IsStarted
        ? null
        : "La génération quotidienne est arrêtée : aucun récap du jour ne sera publié.";
}
