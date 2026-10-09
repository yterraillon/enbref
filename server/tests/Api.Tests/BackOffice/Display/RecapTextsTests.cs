using EnBref.Api.BackOffice.Display;
using EnBref.Api.Features.ReadPublishedRecap;
using EnBref.Api.Shared;

namespace EnBref.Api.Tests.BackOffice.Display;

public class RecapTextsTests
{
    private static readonly Recap Recap = new(new DateOnly(2026, 10, 9), new Dictionary<Category, IReadOnlyList<Brief>>
    {
        [Category.Politics] = [new Brief("A", "a"), new Brief("B", "b")],
    });

    [Test]
    public async Task Daily_recap_of_today_shows_its_counts()
    {
        var result = new ReadPublishedRecapResult("latest.json", Recap, "{}", Error: null, IsFromToday: true);

        await Assert.That(RecapTexts.Daily(result)).IsEqualTo("Publié aujourd’hui · 1 catégorie · 2 brèves.");
    }

    [Test]
    public async Task Daily_recap_of_another_day_says_so()
    {
        var result = new ReadPublishedRecapResult("latest.json", Recap, "{}", Error: null, IsFromToday: false);

        await Assert.That(RecapTexts.Daily(result)).Contains("ne date pas d’aujourd’hui");
    }

    [Test]
    public async Task Missing_daily_recap_names_its_artifact()
    {
        var result = new ReadPublishedRecapResult("latest.json", null, null, "Artefact absent.", IsMissing: true);

        await Assert.That(RecapTexts.Daily(result)).Contains("latest.json n’existe pas");
    }

    [Test]
    public async Task Unreadable_daily_recap_shows_the_error()
    {
        var result = new ReadPublishedRecapResult("latest.json", null, "{}", "Hors contrat.");

        await Assert.That(RecapTexts.Daily(result)).IsEqualTo("Hors contrat.");
    }

    [Test]
    public async Task Meta_of_a_readable_recap()
    {
        var result = new ReadPublishedRecapResult("demo.json", Recap, "{}", Error: null);

        await Assert.That(RecapTexts.Meta(result)).IsEqualTo("Vendredi 9 octobre 2026 · demo.json · 1 catégorie · 2 brèves");
    }

    [Test]
    public async Task Meta_of_a_missing_recap()
    {
        var result = new ReadPublishedRecapResult("test.json", null, null, "Artefact absent.", IsMissing: true);

        await Assert.That(RecapTexts.Meta(result)).IsEqualTo("test.json · pas encore généré");
    }

    [Test]
    [Arguments(RecapType.Daily)]
    [Arguments(RecapType.Demo)]
    [Arguments(RecapType.Test)]
    public async Task Every_recap_type_has_an_empty_state(RecapType type)
    {
        await Assert.That(RecapTexts.EmptyTitle(type)).IsNotEmpty();
        await Assert.That(RecapTexts.EmptyBody(type)).IsNotEmpty();
    }
}
