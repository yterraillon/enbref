namespace EnBref.Api.Tests.Features;

/// <summary>Horloge figée, en UTC.</summary>
internal sealed class StubTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
