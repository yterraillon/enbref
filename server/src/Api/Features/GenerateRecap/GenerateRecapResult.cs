using EnBref.Api.Features.CollectHeadlines;

namespace EnBref.Api.Features.GenerateRecap;

// Provisoire (étape 2.1) : porte le résultat de collecte, en attendant le récap généré.
public sealed record GenerateRecapResult(CollectionResult Collection)
{
    public bool IsSuccessful => Collection.IsSuccessful;
}
