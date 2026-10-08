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

/// <summary>Brève : un titre et un résumé d'une phrase.</summary>
public sealed record Brief(string Title, string Summary);

/// <summary>Récap : sept catégories, une à deux brèves chacune (glossaire § 7).</summary>
public sealed record Recap(DateOnly Date, IReadOnlyDictionary<Category, IReadOnlyList<Brief>> Briefs);
