using System.Globalization;

namespace EnBref.Api.BackOffice.Display;

/// <summary>Dates longues, sans abréviation (design-system/README.md § Contenu et ton).</summary>
public static class FrenchDates
{
    private static readonly CultureInfo French = new("fr-FR");

    /// <summary>« Vendredi 9 octobre ».</summary>
    public static string Day(DateOnly date) => Capitalize(date.ToString("dddd d MMMM", French));

    /// <summary>« vendredi 9 octobre 2026 ».</summary>
    public static string Long(DateOnly date) => date.ToString("dddd d MMMM yyyy", French);

    /// <summary>« 9 octobre 2026 à 9 h 56 », « 9 octobre 2026 à 9 h ».</summary>
    public static string Moment(DateTimeOffset moment)
    {
        var time = moment.Minute == 0 ? $"{moment.Hour} h" : $"{moment.Hour} h {moment.Minute:00}";
        return $"{moment.ToString("d MMMM yyyy", French)} à {time}";
    }

    private static string Capitalize(string text) => string.Concat(char.ToUpper(text[0], French), text[1..]);
}
