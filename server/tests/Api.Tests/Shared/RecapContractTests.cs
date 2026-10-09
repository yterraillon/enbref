using System.Text.Json;
using EnBref.Api.Shared;

namespace EnBref.Api.Tests.Shared;

public class RecapContractTests
{
    private static readonly string[] Categories = ["politics", "international", "economy", "society", "technologyAndScience", "sport", "culture"];

    // Dictionnaire volontairement à l'envers : l'ordre publié ne doit pas en dépendre.
    private static readonly Recap Recap = new(
        new DateOnly(2026, 10, 8),
        Enum.GetValues<Category>().Reverse().ToDictionary(
            category => category,
            IReadOnlyList<Brief> (category) => [new Brief($"Titre {category}", "Une phrase, sans détour.")]));

    [Test]
    public async Task Categories_are_published_in_the_glossary_order()
    {
        using var document = JsonDocument.Parse(RecapContract.Serialize(Recap));

        var categories = document.RootElement.GetProperty("categories").EnumerateArray()
            .Select(category => category.GetProperty("category").GetString())
            .ToList();

        await Assert.That(string.Join(",", categories)).IsEqualTo(string.Join(",", Categories));
    }

    [Test]
    public async Task Fields_follow_the_contract()
    {
        using var document = JsonDocument.Parse(RecapContract.Serialize(Recap));
        var root = document.RootElement;
        var brief = root.GetProperty("categories")[0].GetProperty("briefs")[0];

        await Assert.That(root.GetProperty("date").GetString()).IsEqualTo("2026-10-08");
        await Assert.That(brief.GetProperty("title").GetString()).IsEqualTo("Titre Politics");
        await Assert.That(brief.GetProperty("summary").GetString()).IsEqualTo("Une phrase, sans détour.");
    }

    [Test]
    public async Task Accents_stay_readable()
    {
        await Assert.That(RecapContract.Serialize(Recap)).Contains("détour");
    }

    [Test]
    public async Task A_published_recap_reads_back_identical()
    {
        var recap = RecapContract.Deserialize(RecapContract.Serialize(Recap));

        await Assert.That(recap.Date).IsEqualTo(Recap.Date);
        foreach (var category in Enum.GetValues<Category>())
        {
            await Assert.That(recap.Briefs[category]).IsEquivalentTo(Recap.Briefs[category]);
        }
    }

    [Test]
    [Arguments("""{ "title": "Récap du 8 octobre", "sections": [] }""")]
    [Arguments("""{ "date": "2026-10-08", "categories": [ { "category": "weather", "briefs": [] } ] }""")]
    [Arguments("""{ "date": "2026-10-08", "categories": [ { "category": "sport", "briefs": [ { "title": "Titre" } ] } ] }""")]
    [Arguments("""{ "date": "2026-10-08", "categories": [ { "category": "sport", "briefs": [] }, { "category": "sport", "briefs": [] } ] }""")]
    public async Task A_json_off_contract_is_rejected(string json)
    {
        await Assert.That(() => RecapContract.Deserialize(json)).Throws<JsonException>();
    }
}
