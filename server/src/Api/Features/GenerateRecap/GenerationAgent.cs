using System.Text.Json;
using System.Text.Json.Nodes;
using EnBref.Api.Shared;
using EnBref.Infrastructure.Llm;

namespace EnBref.Api.Features.GenerateRecap;

/// <summary>Écrit un récap à partir des titres collectés, en un seul appel au LLM (ADR-007).</summary>
public sealed class GenerationAgent(ILlmClient llmClient, ILogger<GenerationAgent> logger) : IRecapWriter
{
    /// <summary>Longueur de résumé demandée au LLM (glossaire § 3).</summary>
    public const int TargetSummaryLength = 200;

    /// <summary>Plafond validé : une marge au-delà de la cible, le LLM ne la tenant pas au caractère près.</summary>
    public const int MaxSummaryLength = 300;

    // L'enum Category est la seule liste des catégories : le prompt, le schéma et la lecture en découlent.
    private static readonly Category[] Categories = Enum.GetValues<Category>();

    private static readonly string Prompt = $"""
        Tu rédiges le récap quotidien d'EnBref : l'actualité du jour, lisible en deux minutes.

        Tu reçois la date et la liste des titres d'articles collectés aujourd'hui dans des flux RSS de
        médias français. Un même sujet repris par plusieurs titres est un sujet important.

        Range l'actualité dans ces catégories, toutes obligatoires : {string.Join(", ", Categories.Select(category => category.Label()))}.

        Pour chaque catégorie, écris une ou deux brèves, jamais plus, sur les sujets les plus importants.
        Chaque brève a :
        - un titre court et factuel ;
        - un résumé d'une seule phrase, {TargetSummaryLength} caractères au maximum, qui explique le sujet.

        Appuie-toi uniquement sur les titres fournis : n'invente aucun fait. Écris en français, sur un ton
        neutre. Si aucun titre ne relève d'une catégorie, écris une brève sur le sujet le plus proche.
        """;

    private static readonly string OutputSchema = new JsonObject
    {
        ["type"] = "object",
        ["properties"] = new JsonObject(Categories.Select(category =>
            KeyValuePair.Create(Key(category), (JsonNode?)new JsonObject { ["$ref"] = "#/$defs/briefs" }))),
        ["required"] = new JsonArray([.. Categories.Select(category => (JsonNode?)Key(category))]),
        ["additionalProperties"] = false,
        ["$defs"] = new JsonObject
        {
            ["briefs"] = new JsonObject
            {
                ["type"] = "array",
                ["items"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["title"] = new JsonObject { ["type"] = "string" },
                        ["summary"] = new JsonObject { ["type"] = "string" },
                    },
                    ["required"] = new JsonArray("title", "summary"),
                    ["additionalProperties"] = false,
                },
            },
        },
    }.ToJsonString();

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<RecapWriterResult> WriteAsync(GenerationContext context, CancellationToken cancellationToken)
    {
        var request = new LlmRequest(Prompt, FormatContext(context), OutputSchema);
        var response = await llmClient.SendAsync(request, cancellationToken);

        if (response.Status != LlmStatus.Completed)
        {
            return Fail($"LLM {response.Status} : {response.Error}");
        }

        Dictionary<string, List<Brief>?>? output;
        try
        {
            output = JsonSerializer.Deserialize<Dictionary<string, List<Brief>?>>(response.Content, JsonOptions);
        }
        catch (JsonException exception)
        {
            return Fail($"Réponse du LLM illisible : {exception.Message}");
        }

        if (output is null)
        {
            return Fail("Réponse du LLM vide.");
        }

        // Clés lues sans tenir compte de la casse, comme les propriétés des brèves.
        var briefs = Categories.ToDictionary(
            category => category,
            IReadOnlyList<Brief> (category) => output
                .FirstOrDefault(pair => string.Equals(pair.Key, Key(category), StringComparison.OrdinalIgnoreCase)).Value ?? []);

        // L'API n'impose ni le nombre de brèves ni la longueur du résumé : on les vérifie ici.
        var violations = briefs.SelectMany(pair => Violations(pair.Key, pair.Value)).ToList();
        if (violations.Count > 0)
        {
            return Fail($"Récap non conforme : {string.Join(" ; ", violations)}");
        }

        return new RecapWriterResult(new Recap(context.Date, briefs), Error: null);
    }

    // Même convention que le contrat publié (RecapContract) : camelCase.
    private static string Key(Category category) => JsonNamingPolicy.CamelCase.ConvertName(category.ToString());

    private static string FormatContext(GenerationContext context) =>
        $"""
        Date : {context.Date:yyyy-MM-dd}

        Titres collectés :
        {string.Join('\n', context.Headlines.Select(headline => $"- {headline}"))}
        """;

    private static IEnumerable<string> Violations(Category category, IReadOnlyList<Brief> briefs)
    {
        if (briefs.Count is < 1 or > 2)
        {
            yield return $"{category} : {briefs.Count} brève(s)";
        }

        foreach (var brief in briefs)
        {
            if (string.IsNullOrWhiteSpace(brief.Title) || string.IsNullOrWhiteSpace(brief.Summary))
            {
                yield return $"{category} : brève sans titre ou sans résumé";
            }
            else if (brief.Summary.Length > MaxSummaryLength)
            {
                yield return $"{category} : résumé de {brief.Summary.Length} caractères";
            }
        }
    }

    private RecapWriterResult Fail(string error)
    {
        logger.LogError("Agent de génération en échec : {Error}", error);
        return new RecapWriterResult(Recap: null, error);
    }

}
