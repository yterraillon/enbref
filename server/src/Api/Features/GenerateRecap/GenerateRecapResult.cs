using EnBref.Api.Features.CollectHeadlines;

namespace EnBref.Api.Features.GenerateRecap;

// Provisoire : porte la collecte et la réponse brute du LLM, en attendant le récap généré.
public sealed record GenerateRecapResult(CollectionResult Collection, string? LlmResponse)
{
    public bool IsSuccessful => Collection.IsSuccessful;
}
