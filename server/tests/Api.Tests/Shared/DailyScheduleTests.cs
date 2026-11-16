using EnBref.Api.Shared;

namespace EnBref.Api.Tests.Shared;

public class DailyScheduleTests
{
    private static readonly TimeZoneInfo Paris = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");
    private static readonly TimeOnly FivePm = new(17, 0);

    private static DateTimeOffset InParis(int month, int day, int hour, int minute = 0)
    {
        var local = new DateTime(2027, month, day, hour, minute, 0);
        return new DateTimeOffset(local, Paris.GetUtcOffset(local));
    }

    [Test]
    public async Task Next_occurrence_is_today_before_the_time()
    {
        var next = DailySchedule.NextOccurrence(InParis(1, 10, 10), FivePm, Paris);

        await Assert.That(next).IsEqualTo(InParis(1, 10, 17));
    }

    [Test]
    [Arguments(17, 0)]
    [Arguments(18, 0)]
    public async Task Next_occurrence_is_tomorrow_once_the_time_is_reached(int hour, int minute)
    {
        var next = DailySchedule.NextOccurrence(InParis(1, 10, hour, minute), FivePm, Paris);

        await Assert.That(next).IsEqualTo(InParis(1, 11, 17));
    }

    [Test]
    public async Task Next_occurrence_follows_the_switch_to_summer_time()
    {
        var next = DailySchedule.NextOccurrence(InParis(3, 27, 18), FivePm, Paris);

        await Assert.That(next.Offset).IsEqualTo(TimeSpan.FromHours(2));
        await Assert.That(next.UtcDateTime).IsEqualTo(new DateTime(2027, 3, 28, 15, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public async Task A_skipped_hour_is_shifted_by_one_hour()
    {
        var next = DailySchedule.NextOccurrence(InParis(3, 27, 18), new TimeOnly(2, 30), Paris);

        await Assert.That(next.UtcDateTime).IsEqualTo(new DateTime(2027, 3, 28, 1, 30, 0, DateTimeKind.Utc));
    }

    [Test]
    public async Task Check_after_midnight_belongs_to_the_previous_day_generation()
    {
        // Génération à 23:30, vérification une heure plus tard : à 00:10 le 11, elle est encore due, pour le 10.
        var (due, generationDate) = DailySchedule.NextRun(InParis(1, 11, 0, 10), new TimeOnly(23, 30), TimeSpan.FromHours(1), Paris);

        await Assert.That(due).IsEqualTo(InParis(1, 11, 0, 30));
        await Assert.That(generationDate).IsEqualTo(new DateOnly(2027, 1, 10));
    }

    [Test]
    public async Task Check_is_one_hour_after_the_generation()
    {
        var (due, generationDate) = DailySchedule.NextRun(InParis(1, 10, 17, 30), FivePm, TimeSpan.FromHours(1), Paris);

        await Assert.That(due).IsEqualTo(InParis(1, 10, 18));
        await Assert.That(generationDate).IsEqualTo(new DateOnly(2027, 1, 10));
    }

    [Test]
    public async Task Now_is_read_in_the_zone()
    {
        // 23:30 UTC le 10 janvier = 00:30 le 11 à Paris : 17 h tombe le 11, pas le 12.
        var now = new DateTimeOffset(2027, 1, 10, 23, 30, 0, TimeSpan.Zero);

        await Assert.That(DailySchedule.NextOccurrence(now, FivePm, Paris)).IsEqualTo(InParis(1, 11, 17));
    }
}
