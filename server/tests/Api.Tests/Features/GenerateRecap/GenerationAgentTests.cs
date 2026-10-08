using System.Text.Json;
using EnBref.Api.Features.GenerateRecap;
using EnBref.Infrastructure.Llm;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnBref.Api.Tests.Features.GenerateRecap;

public class GenerationAgentTests
{
    private static readonly GenerationContext Context = new(new DateOnly(2026, 10, 5), ["Titre A", "Titre B"]);

    private static readonly string[] Categories = ["politics", "international", "economy", "society", "technologyAndScience", "sport", "culture"];

    /// <summary>Sortie conforme : une brève par catégorie, sauf exceptions passées en paramètre.</summary>
    internal static string Output(Func<string, int>? briefCount = null, string summary = "Une phrase.") =>
        JsonSerializer.Serialize(Categories.ToDictionary(
            category => category,
            category => Enumerable.Range(0, briefCount?.Invoke(category) ?? 1)
                .Select(index => new { title = $"{category} {index}", summary })
                .ToList()));

    private static GenerationAgent Agent(StubLlmClient llm) => new(llm, NullLogger<GenerationAgent>.Instance);

    [Test]
    public async Task Valid_output_becomes_a_recap_with_the_seven_categories_in_order()
    {
        var result = await Agent(StubLlmClient.Completed(Output())).WriteAsync(Context, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(result.Recap!.Date).IsEqualTo(Context.Date);
        await Assert.That(result.Recap.Briefs.Keys).IsEquivalentTo(Enum.GetValues<Category>());
        await Assert.That(result.Recap.Briefs[Category.Economy][0]).IsEqualTo(new Brief("economy 0", "Une phrase."));
    }

    [Test]
    public async Task Request_carries_the_headlines_and_an_output_schema()
    {
        var llm = StubLlmClient.Completed(Output());

        await Agent(llm).WriteAsync(Context, CancellationToken.None);

        await Assert.That(llm.LastRequest!.Context).Contains("- Titre A");
        await Assert.That(llm.LastRequest.Context).Contains("- Titre B");
        await Assert.That(llm.LastRequest.Context).Contains("2026-10-05");
        await Assert.That(llm.LastRequest.OutputSchema).IsNotNull();
    }

    [Test]
    public async Task Output_schema_requires_exactly_the_seven_categories()
    {
        var llm = StubLlmClient.Completed(Output());

        await Agent(llm).WriteAsync(Context, CancellationToken.None);

        using var schema = JsonDocument.Parse(llm.LastRequest!.OutputSchema!);
        var properties = schema.RootElement.GetProperty("properties").EnumerateObject().Select(property => property.Name);
        var required = schema.RootElement.GetProperty("required").EnumerateArray().Select(key => key.GetString());
        await Assert.That(string.Join(",", properties)).IsEqualTo(string.Join(",", Categories));
        await Assert.That(string.Join(",", required)).IsEqualTo(string.Join(",", Categories));
    }

    [Test]
    public async Task Prompt_names_the_categories_in_the_glossary_order()
    {
        var llm = StubLlmClient.Completed(Output());

        await Agent(llm).WriteAsync(Context, CancellationToken.None);

        await Assert.That(llm.LastRequest!.Prompt)
            .Contains("Politique, International, Économie, Société, Technologies & Science, Sport, Culture.");
    }

    [Test]
    [Arguments(0)]
    [Arguments(3)]
    public async Task Category_with_a_wrong_number_of_briefs_fails(int count)
    {
        var output = Output(category => category == "economy" ? count : 1);

        var result = await Agent(StubLlmClient.Completed(output)).WriteAsync(Context, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(result.Error).Contains("Economy");
    }

    [Test]
    public async Task Two_briefs_in_a_category_are_accepted()
    {
        var output = Output(category => category == "sport" ? 2 : 1);

        var result = await Agent(StubLlmClient.Completed(output)).WriteAsync(Context, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
    }

    [Test]
    public async Task Summary_over_the_limit_fails()
    {
        var output = Output(summary: new string('a', GenerationAgent.MaxSummaryLength + 1));

        var result = await Agent(StubLlmClient.Completed(output)).WriteAsync(Context, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
    }

    [Test]
    public async Task Unreadable_output_fails()
    {
        var result = await Agent(StubLlmClient.Completed("pas du JSON")).WriteAsync(Context, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
    }

    [Test]
    [Arguments(LlmStatus.Refused)]
    [Arguments(LlmStatus.Truncated)]
    [Arguments(LlmStatus.Unavailable)]
    [Arguments(LlmStatus.Rejected)]
    public async Task Incomplete_llm_response_fails_and_names_the_status(LlmStatus status)
    {
        var llm = new StubLlmClient(new LlmResponse(status, Output(), "cause"));

        var result = await Agent(llm).WriteAsync(Context, CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsFalse();
        await Assert.That(result.Error).Contains(status.ToString());
    }
}
