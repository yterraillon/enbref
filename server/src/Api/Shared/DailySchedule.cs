namespace EnBref.Api.Shared;

/// <summary>Échéances quotidiennes du job quotidien et de la vérification de publication.</summary>
public static class DailySchedule
{
    /// <summary>
    /// Prochain passage à <paramref name="time"/>, heure locale de <paramref name="zone"/>, strictement
    /// après <paramref name="now"/>. Une heure sautée au passage à l'heure d'été est décalée d'une heure.
    /// </summary>
    public static DateTimeOffset NextOccurrence(DateTimeOffset now, TimeOnly time, TimeZoneInfo zone)
    {
        var localNow = TimeZoneInfo.ConvertTime(now, zone);
        var day = DateOnly.FromDateTime(localNow.DateTime);

        var occurrence = At(day, time, zone);
        return occurrence > now ? occurrence : At(day.AddDays(1), time, zone);
    }

    /// <summary>
    /// Prochaine exécution d'un job décalé de <paramref name="offset"/> après l'heure de génération,
    /// strictement après <paramref name="now"/>, et la date de la génération à laquelle elle se rattache.
    /// Une vérification à 00:30 pour une génération à 23:30 porte sur la veille.
    /// </summary>
    public static (DateTimeOffset Due, DateOnly GenerationDate) NextRun(DateTimeOffset now, TimeOnly time, TimeSpan offset, TimeZoneInfo zone)
    {
        var generation = NextOccurrence(now - offset, time, zone);
        return (generation + offset, DateOnly.FromDateTime(generation.DateTime));
    }

    private static DateTimeOffset At(DateOnly day, TimeOnly time, TimeZoneInfo zone)
    {
        var local = day.ToDateTime(time);
        if (zone.IsInvalidTime(local))
        {
            local = local.AddHours(1);
        }

        return new DateTimeOffset(local, zone.GetUtcOffset(local));
    }
}
