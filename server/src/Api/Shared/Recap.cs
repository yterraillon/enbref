namespace EnBref.Api.Shared;

/// <summary>Type de récap — voir docs/ubiquitous-language.md.</summary>
public enum RecapType
{
    Daily,
    Demo,
    Test,
}

/// <summary>Catégorie — liste fixe, fermée, dans l'ordre du glossaire (docs/ubiquitous-language.md § 3).</summary>
public enum Category
{
    Politics,
    International,
    Economy,
    Society,
    TechnologyAndScience,
    Sport,
    Culture,
}

public static class RecapTypes
{
    /// <summary>Libellé UI du type de récap (glossaire § 2).</summary>
    public static string Label(this RecapType type) => type switch
    {
        RecapType.Daily => "Récap du jour",
        RecapType.Demo => "Récap de démo",
        RecapType.Test => "Récap de test",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Type de récap sans libellé."),
    };

    // Seul endroit qui nomme un artefact publié. L'artefact découle du type, jamais d'un paramètre
    // d'appel ni de la configuration : le récap de test n'atteint que test.json (ADR-007).
    public static string Artifact(this RecapType type) => type switch
    {
        RecapType.Daily => "latest.json",
        RecapType.Demo => "demo.json",
        RecapType.Test => "test.json",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Type de récap sans artefact."),
    };
}

public static class CategoryLabels
{
    /// <summary>Libellé UI de la catégorie (glossaire § 3).</summary>
    public static string Label(this Category category) => category switch
    {
        Category.Politics => "Politique",
        Category.International => "International",
        Category.Economy => "Économie",
        Category.Society => "Société",
        Category.TechnologyAndScience => "Technologies & Science",
        Category.Sport => "Sport",
        Category.Culture => "Culture",
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Catégorie sans libellé."),
    };
}

/// <summary>Brève : un titre et un résumé d'une phrase.</summary>
public sealed record Brief(string Title, string Summary);

/// <summary>Récap : sept catégories, une à deux brèves chacune (glossaire § 7).</summary>
public sealed record Recap(DateOnly Date, IReadOnlyDictionary<Category, IReadOnlyList<Brief>> Briefs);
