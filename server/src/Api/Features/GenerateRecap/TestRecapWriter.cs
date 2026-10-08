namespace EnBref.Api.Features.GenerateRecap;

/// <summary>Récap de test : écrit sans LLM à partir de la fausse source, il ne prouve que la publication (ADR-007).</summary>
public sealed class TestRecapWriter : IRecapWriter
{
    public const string Summary = "Récap de test : aucun LLM appelé.";

    /// <summary>Une brève par catégorie, dont le titre reprend un titre collecté.</summary>
    public Task<RecapWriterResult> WriteAsync(GenerationContext context, CancellationToken cancellationToken) =>
        Task.FromResult(new RecapWriterResult(
            new Recap(
                context.Date,
                Enum.GetValues<Category>().Select((category, index) => (category, index)).ToDictionary(
                    item => item.category,
                    IReadOnlyList<Brief> (item) => [new Brief(context.Headlines[item.index % context.Headlines.Count], Summary)])),
            Error: null));
}
