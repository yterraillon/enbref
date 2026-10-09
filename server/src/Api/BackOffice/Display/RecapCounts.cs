using EnBref.Api.Shared;

namespace EnBref.Api.BackOffice.Display;

/// <summary>Comptes en toutes lettres autour du chiffre : « 5 catégories · 10 brèves ».</summary>
public static class RecapCounts
{
    public static string Describe(Recap recap)
    {
        var categories = CategoriesWithBriefs(recap);
        return $"{Count(categories.Count, "catégorie", "catégories")} · {Briefs(categories.Sum(category => category.Briefs.Count))}";
    }

    /// <summary>Catégories affichées : celles qui ont au moins une brève, dans l'ordre du glossaire.</summary>
    public static IReadOnlyList<(Category Category, IReadOnlyList<Brief> Briefs)> CategoriesWithBriefs(Recap recap) =>
        Enum.GetValues<Category>()
            .Select(category => (Category: category, Briefs: recap.Briefs.GetValueOrDefault(category) ?? []))
            .Where(category => category.Briefs.Count > 0)
            .ToList();

    public static string Briefs(int count) => Count(count, "brève", "brèves");

    public static string Count(int count, string singular, string plural) => $"{count} {(count > 1 ? plural : singular)}";
}
