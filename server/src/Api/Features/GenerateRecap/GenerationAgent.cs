using System.Text.Json;
using EnBref.Infrastructure.Llm;

namespace EnBref.Api.Features.GenerateRecap;

/// <summary>Ce que l'agent de génération reçoit pour écrire un récap.</summary>
public sealed record GenerationContext(DateOnly Date, IReadOnlyList<string> Headlines);

public sealed record GenerationAgentResult(Recap? Recap, string? Error)
{
    public bool IsSuccessful => Recap is not null;
}

/// <summary>Écrit un récap à partir des titres collectés, en un seul appel au LLM (ADR-007).</summary>
public sealed class GenerationAgent(ILlmClient llmClient, ILogger<GenerationAgent> logger)
{
    public const int MaxSummaryLength = 200;

    private const string Prompt = """
        Tu rédiges le récap quotidien d'EnBref : l'actualité du jour, lisible en deux minutes.

        Tu reçois la date et la liste des titres d'articles collectés aujourd'hui dans des flux RSS de
        médias français. Un même sujet repris par plusieurs titres est un sujet important.

        Range l'actualité dans ces sept catégories, toutes obligatoires : Politique, International,
        Économie, Société, Technologies & Science, Sport, Culture.

        Pour chaque catégorie, écris une ou deux brèves, jamais plus, sur les sujets les plus importants.
        Chaque brève a :
        - un titre court et factuel ;
        - un résumé d'une seule phrase, 200 caractères au maximum, qui explique le sujet.

        Appuie-toi uniquement sur les titres fournis : n'invente aucun fait. Écris en français, sur un ton
        neutre. Si aucun titre ne relève d'une catégorie, écris une brève sur le sujet le plus proche.
        """;

    private const string OutputSchema = """
        {
          "type": "object",
          "properties": {
            "politics": { "$ref": "#/$defs/briefs" },
            "international": { "$ref": "#/$defs/briefs" },
            "economy": { "$ref": "#/$defs/briefs" },
            "society": { "$ref": "#/$defs/briefs" },
            "technologyAndScience": { "$ref": "#/$defs/briefs" },
            "sport": { "$ref": "#/$defs/briefs" },
            "culture": { "$ref": "#/$defs/briefs" }
          },
          "required": ["politics", "international", "economy", "society", "technologyAndScience", "sport", "culture"],
          "additionalProperties": false,
          "$defs": {
            "briefs": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "title": { "type": "string" },
                  "summary": { "type": "string" }
                },
                "required": ["title", "summary"],
                "additionalProperties": false
              }
            }
          }
        }
        """;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<GenerationAgentResult> WriteAsync(GenerationContext context, CancellationToken cancellationToken)
    {
        var request = new LlmRequest(Prompt, FormatContext(context), OutputSchema);
        var response = await llmClient.SendAsync(request, cancellationToken);

        if (response.Status != LlmStatus.Completed)
        {
            return Fail($"LLM {response.Status} : {response.Error}");
        }

        RecapOutput? output;
        try
        {
            output = JsonSerializer.Deserialize<RecapOutput>(response.Content, JsonOptions);
        }
        catch (JsonException exception)
        {
            return Fail($"Réponse du LLM illisible : {exception.Message}");
        }

        if (output is null)
        {
            return Fail("Réponse du LLM vide.");
        }

        var briefs = new Dictionary<Category, IReadOnlyList<Brief>>
        {
            [Category.Politics] = output.Politics ?? [],
            [Category.International] = output.International ?? [],
            [Category.Economy] = output.Economy ?? [],
            [Category.Society] = output.Society ?? [],
            [Category.TechnologyAndScience] = output.TechnologyAndScience ?? [],
            [Category.Sport] = output.Sport ?? [],
            [Category.Culture] = output.Culture ?? [],
        };

        // L'API n'impose ni le nombre de brèves ni la longueur du résumé : on les vérifie ici.
        var violations = briefs.SelectMany(pair => Violations(pair.Key, pair.Value)).ToList();
        if (violations.Count > 0)
        {
            return Fail($"Récap non conforme : {string.Join(" ; ", violations)}");
        }

        return new GenerationAgentResult(new Recap(context.Date, briefs), Error: null);
    }

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

    private GenerationAgentResult Fail(string error)
    {
        logger.LogError("Agent de génération en échec : {Error}", error);
        return new GenerationAgentResult(Recap: null, error);
    }

    // Calqué sur OutputSchema.
    private sealed record RecapOutput(
        List<Brief>? Politics,
        List<Brief>? International,
        List<Brief>? Economy,
        List<Brief>? Society,
        List<Brief>? TechnologyAndScience,
        List<Brief>? Sport,
        List<Brief>? Culture);
}
