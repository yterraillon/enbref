using EnBref.Api.Features.GenerateRecap;

namespace EnBref.Api.Tests.Features.GenerateRecap;

public class TestRecapWriterTests
{
    private static readonly DateOnly Date = new(2026, 10, 8);

    [Test]
    public async Task Writes_one_brief_per_category()
    {
        var result = await new TestRecapWriter().WriteAsync(new GenerationContext(Date, ["A"]), CancellationToken.None);

        await Assert.That(result.IsSuccessful).IsTrue();
        await Assert.That(result.Recap!.Date).IsEqualTo(Date);
        await Assert.That(result.Recap.Briefs.Keys).IsEquivalentTo(Enum.GetValues<Category>());
        await Assert.That(result.Recap.Briefs.Values.All(briefs => briefs.Count == 1)).IsTrue();
    }

    [Test]
    public async Task Brief_titles_reuse_the_collected_headlines()
    {
        var result = await new TestRecapWriter().WriteAsync(new GenerationContext(Date, ["A", "B"]), CancellationToken.None);

        var titles = result.Recap!.Briefs.Values.SelectMany(briefs => briefs).Select(brief => brief.Title).ToList();
        await Assert.That(titles.Distinct()).IsEquivalentTo(["A", "B"]);
    }
}
