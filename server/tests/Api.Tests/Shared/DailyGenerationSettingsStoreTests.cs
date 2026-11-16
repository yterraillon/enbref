using EnBref.Api.Shared;
using EnBref.Api.Tests.Features;

namespace EnBref.Api.Tests.Shared;

public class DailyGenerationSettingsStoreTests
{
    private readonly TemporarySettings settings = new();

    [After(Test)]
    public void DeleteSettings() => settings.Dispose();

    [Test]
    public async Task Daily_generation_is_stopped_at_five_pm_by_default()
    {
        await Assert.That(settings.Store().Current).IsEqualTo(new DailyGenerationSettings(false, new TimeOnly(17, 0)));
    }

    [Test]
    public async Task Settings_survive_a_restart()
    {
        var store = settings.Store();
        store.Start();
        store.SetTime(new TimeOnly(6, 30));

        await Assert.That(settings.Store().Current).IsEqualTo(new DailyGenerationSettings(true, new TimeOnly(6, 30)));
    }

    [Test]
    public async Task Daily_generation_starts_then_stops_again()
    {
        var store = settings.Store();

        store.Start();
        await Assert.That(store.Current.IsStarted).IsTrue();

        store.Stop();
        await Assert.That(store.Current.IsStarted).IsFalse();
    }

    [Test]
    public async Task Unreadable_settings_fall_back_to_default()
    {
        settings.Write("{ pas du json");

        await Assert.That(settings.Store().Current).IsEqualTo(DailyGenerationSettings.Default);
    }

    [Test]
    public async Task Changes_are_announced_with_the_new_settings()
    {
        var store = settings.Store();
        DailyGenerationSettings? announced = null;
        store.Changed += changed => announced = changed;

        store.SetTime(new TimeOnly(8, 15, 42));

        await Assert.That(announced).IsEqualTo(new DailyGenerationSettings(false, new TimeOnly(8, 15)));
    }

    [Test]
    public async Task Unchanged_settings_are_not_announced()
    {
        var store = settings.Store();
        var announcements = 0;
        store.Changed += _ => announcements++;

        store.Stop();

        await Assert.That(announcements).IsEqualTo(0);
    }
}
