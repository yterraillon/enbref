using EnBref.Api.BackOffice.Display;
using EnBref.Api.Features.ReadPublishedRecap;
using EnBref.Api.Shared;

namespace EnBref.Api.Tests.BackOffice.Display;

public class LatestRecapTests
{
    private static ReadPublishedRecapResult Read(RecapType type, int day) =>
        new(type.Artifact(), new Recap(new DateOnly(2026, 10, day), new Dictionary<Category, IReadOnlyList<Brief>>()), "{}", Error: null);

    private static ReadPublishedRecapResult Missing(RecapType type) =>
        new(type.Artifact(), Recap: null, Content: null, "Artefact absent.", IsMissing: true);

    [Test]
    public async Task Most_recent_recap_wins()
    {
        var reads = new Dictionary<RecapType, ReadPublishedRecapResult>
        {
            [RecapType.Daily] = Read(RecapType.Daily, 8),
            [RecapType.Demo] = Read(RecapType.Demo, 9),
            [RecapType.Test] = Read(RecapType.Test, 7),
        };

        await Assert.That(LatestRecap.Pick(reads)).IsEqualTo(RecapType.Demo);
    }

    [Test]
    public async Task Same_date_prefers_daily_then_demo_then_test()
    {
        var reads = new Dictionary<RecapType, ReadPublishedRecapResult>
        {
            [RecapType.Test] = Read(RecapType.Test, 9),
            [RecapType.Demo] = Read(RecapType.Demo, 9),
            [RecapType.Daily] = Read(RecapType.Daily, 9),
        };

        await Assert.That(LatestRecap.Pick(reads)).IsEqualTo(RecapType.Daily);
    }

    [Test]
    public async Task Unreadable_recaps_are_ignored()
    {
        var reads = new Dictionary<RecapType, ReadPublishedRecapResult>
        {
            [RecapType.Daily] = Missing(RecapType.Daily),
            [RecapType.Test] = Read(RecapType.Test, 1),
        };

        await Assert.That(LatestRecap.Pick(reads)).IsEqualTo(RecapType.Test);
    }

    [Test]
    public async Task No_readable_recap_gives_none()
    {
        var reads = new Dictionary<RecapType, ReadPublishedRecapResult> { [RecapType.Daily] = Missing(RecapType.Daily) };

        await Assert.That(LatestRecap.Pick(reads)).IsNull();
    }
}
