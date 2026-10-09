using EnBref.Api.BackOffice.Display;
using EnBref.Api.Shared;

namespace EnBref.Api.Tests.BackOffice.Display;

public class RecapCountsTests
{
    [Test]
    public async Task Counts_categories_with_briefs_and_all_briefs()
    {
        var recap = new Recap(new DateOnly(2026, 10, 9), new Dictionary<Category, IReadOnlyList<Brief>>
        {
            [Category.Politics] = [new Brief("A", "a"), new Brief("B", "b")],
            [Category.Economy] = [new Brief("C", "c")],
            [Category.Sport] = [],
        });

        await Assert.That(RecapCounts.Describe(recap)).IsEqualTo("2 catégories · 3 brèves");
    }

    [Test]
    public async Task Categories_with_briefs_follow_the_glossary_order()
    {
        var recap = new Recap(new DateOnly(2026, 10, 9), new Dictionary<Category, IReadOnlyList<Brief>>
        {
            [Category.Culture] = [new Brief("C", "c")],
            [Category.Sport] = [],
            [Category.Politics] = [new Brief("A", "a")],
        });

        var categories = string.Join(",", RecapCounts.CategoriesWithBriefs(recap).Select(category => category.Category));

        await Assert.That(categories).IsEqualTo("Politics,Culture");
    }

    [Test]
    [Arguments(0, "0 brève")]
    [Arguments(1, "1 brève")]
    [Arguments(2, "2 brèves")]
    public async Task Plural_starts_at_two(int count, string expected)
    {
        await Assert.That(RecapCounts.Briefs(count)).IsEqualTo(expected);
    }
}
