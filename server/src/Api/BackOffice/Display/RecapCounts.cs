using EnBref.Api.Shared;

namespace EnBref.Api.BackOffice.Display;

/// <summary>Comptes en toutes lettres autour du chiffre : « 5 catégories · 10 brèves ».</summary>
public static class RecapCounts
{
    public static string Describe(Recap recap)
    {
        var categories = recap.Briefs.Count(category => category.Value.Count > 0);
        return $"{Count(categories, "catégorie", "catégories")} · {Briefs(recap.Briefs.Values.Sum(briefs => briefs.Count))}";
    }

    public static string Briefs(int count) => Count(count, "brève", "brèves");

    public static string Count(int count, string singular, string plural) => $"{count} {(count > 1 ? plural : singular)}";
}
