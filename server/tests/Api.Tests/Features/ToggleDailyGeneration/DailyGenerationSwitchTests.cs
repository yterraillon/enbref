using EnBref.Api.Features.ToggleDailyGeneration;

namespace EnBref.Api.Tests.Features.ToggleDailyGeneration;

public class DailyGenerationSwitchTests
{
    [Test]
    public async Task Daily_generation_is_started_by_default()
    {
        await Assert.That(new DailyGenerationSwitch().IsStarted).IsTrue();
    }

    [Test]
    public async Task Daily_generation_stops_then_starts_again()
    {
        var dailyGeneration = new DailyGenerationSwitch();

        dailyGeneration.Stop();
        await Assert.That(dailyGeneration.IsStarted).IsFalse();

        dailyGeneration.Start();
        await Assert.That(dailyGeneration.IsStarted).IsTrue();
    }
}
