namespace EnBref.Api.Features.GenerateRecap;

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

/// <summary>Étape d'une génération — voir docs/ubiquitous-language.md.</summary>
public enum GenerationStep
{
    Collection,
    Generation,
    Publication,
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
