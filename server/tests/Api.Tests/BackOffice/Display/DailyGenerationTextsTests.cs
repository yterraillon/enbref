using EnBref.Api.BackOffice.Display;
using EnBref.Api.Shared;

namespace EnBref.Api.Tests.BackOffice.Display;

public class DailyGenerationTextsTests
{
    private static readonly TimeZoneInfo Paris = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");

    // Lundi 16 novembre 2026, 10 h à Paris.
    private static readonly DateTimeOffset Now = new(2026, 11, 16, 10, 0, 0, TimeSpan.FromHours(1));

    [Test]
    [Arguments(17, 0, "17 h")]
    [Arguments(6, 30, "6 h 30")]
    [Arguments(9, 5, "9 h 05")]
    public async Task Time_is_spelled_in_french(int hour, int minute, string expected)
    {
        await Assert.That(DailyGenerationTexts.Time(new TimeOnly(hour, minute))).IsEqualTo(expected);
    }

    [Test]
    [Arguments(17, 0, "Aujourd’hui à 17 h")]
    [Arguments(6, 30, "Demain à 6 h 30")]
    public async Task Next_generation_is_today_or_tomorrow(int hour, int minute, string expected)
    {
        var settings = new DailyGenerationSettings(IsStarted: true, new TimeOnly(hour, minute));

        await Assert.That(DailyGenerationTexts.NextGeneration(settings, Now, Paris)).IsEqualTo(expected);
    }

    [Test]
    public async Task Stopped_daily_generation_has_no_next_generation()
    {
        var settings = DailyGenerationSettings.Default with { IsStarted = false };

        await Assert.That(DailyGenerationTexts.NextGeneration(settings, Now, Paris)).IsEqualTo("Aucune, génération quotidienne arrêtée");
    }

    [Test]
    [Arguments("enbref", "enbref")]
    [Arguments("", "Non configuré")]
    [Arguments(null, "Non configuré")]
    public async Task Topic_says_when_alerts_are_not_configured(string? topic, string expected)
    {
        await Assert.That(DailyGenerationTexts.Topic(topic)).IsEqualTo(expected);
    }

    [Test]
    public async Task Banner_only_shows_when_daily_generation_is_stopped()
    {
        await Assert.That(DailyGenerationTexts.Banner(DailyGenerationSettings.Default with { IsStarted = true })).IsNull();
        await Assert.That(DailyGenerationTexts.Banner(DailyGenerationSettings.Default with { IsStarted = false }))
            .IsEqualTo("La génération quotidienne est arrêtée : aucun récap du jour ne sera publié.");
    }
}
