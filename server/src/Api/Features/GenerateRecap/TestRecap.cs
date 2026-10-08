namespace EnBref.Api.Features.GenerateRecap;

/// <summary>Récap de test : écrit sans LLM à partir de la fausse source, il ne prouve que la publication (ADR-007).</summary>
public static class TestRecap
{
    public const string Summary = "Récap de test : aucun LLM appelé.";

    /// <summary>Une brève par catégorie, dont le titre reprend un titre collecté.</summary>
    public static Recap From(DateOnly date, IReadOnlyList<string> headlines) => new(
        date,
        Enum.GetValues<Category>().Select((category, index) => (category, index)).ToDictionary(
            item => item.category,
            IReadOnlyList<Brief> (item) => [new Brief(headlines[item.index % headlines.Count], Summary)]));
}
