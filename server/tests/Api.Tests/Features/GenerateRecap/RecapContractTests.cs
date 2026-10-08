using System.Text.Json;
using EnBref.Api.Features.GenerateRecap;

namespace EnBref.Api.Tests.Features.GenerateRecap;

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
}
