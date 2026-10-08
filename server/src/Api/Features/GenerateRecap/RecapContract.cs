using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnBref.Api.Features.GenerateRecap;

/// <summary>Le contrat publié (ADR-008) : la seule forme que connaissent les clients.</summary>
public static class RecapContract
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        WriteIndented = true,
        // demo.json est relu à la main : les accents restent lisibles.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Serialize(Recap recap) => JsonSerializer.Serialize(
        new RecapDocument(
            recap.Date,
            // Tableau, pas objet : l'ordre du glossaire voyage avec le JSON.
            Enum.GetValues<Category>()
                .Select(category => new CategoryDocument(category, recap.Briefs.GetValueOrDefault(category) ?? []))
                .ToList()),
        JsonOptions);

    private sealed record RecapDocument(DateOnly Date, IReadOnlyList<CategoryDocument> Categories);

    private sealed record CategoryDocument(Category Category, IReadOnlyList<Brief> Briefs);
}
