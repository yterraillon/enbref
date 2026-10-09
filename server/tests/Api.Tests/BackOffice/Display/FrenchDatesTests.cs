using EnBref.Api.BackOffice.Display;

namespace EnBref.Api.Tests.BackOffice.Display;

public class FrenchDatesTests
{
    [Test]
    public async Task Day_is_capitalized_without_year()
    {
        await Assert.That(FrenchDates.Day(new DateOnly(2026, 10, 9))).IsEqualTo("Vendredi 9 octobre");
    }

    [Test]
    public async Task Long_date_spells_out_the_day_and_year()
    {
        await Assert.That(FrenchDates.Long(new DateOnly(2026, 10, 9))).IsEqualTo("vendredi 9 octobre 2026");
    }

    [Test]
    [Arguments(9, 56, "9 octobre 2026 à 9 h 56")]
    [Arguments(9, 5, "9 octobre 2026 à 9 h 05")]
    [Arguments(17, 0, "9 octobre 2026 à 17 h")]
    public async Task Moment_follows_french_time_style(int hour, int minute, string expected)
    {
        var moment = new DateTimeOffset(2026, 10, 9, hour, minute, 0, TimeSpan.FromHours(2));

        await Assert.That(FrenchDates.Moment(moment)).IsEqualTo(expected);
    }
}
