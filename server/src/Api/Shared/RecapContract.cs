using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnBref.Api.Shared;

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
        // À la relecture, un champ absent ou null est un écart au contrat, pas une valeur par défaut.
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
    };

    public static string Serialize(Recap recap) => JsonSerializer.Serialize(
        new RecapDocument(
            recap.Date,
            // Tableau, pas objet : l'ordre du glossaire voyage avec le JSON.
            Enum.GetValues<Category>()
                .Select(category => new CategoryDocument(category, recap.Briefs.GetValueOrDefault(category) ?? []))
                .ToList()),
        JsonOptions);

    /// <exception cref="JsonException">Le JSON ne suit pas le contrat.</exception>
    public static Recap Deserialize(string json)
    {
        var document = JsonSerializer.Deserialize<RecapDocument>(json, JsonOptions)
                       ?? throw new JsonException("Récap absent du JSON.");

        var briefs = new Dictionary<Category, IReadOnlyList<Brief>>();
        foreach (var category in document.Categories)
        {
            if (!briefs.TryAdd(category.Category, category.Briefs))
            {
                throw new JsonException($"Catégorie en double : {category.Category}.");
            }
        }

        return new Recap(document.Date, briefs);
    }

    private sealed record RecapDocument(DateOnly Date, IReadOnlyList<CategoryDocument> Categories);

    private sealed record CategoryDocument(Category Category, IReadOnlyList<Brief> Briefs);
}
