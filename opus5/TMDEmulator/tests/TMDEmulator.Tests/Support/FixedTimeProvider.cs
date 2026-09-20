namespace TMDEmulator.Tests.Support;

internal sealed class FixedTimeProvider : TimeProvider
{
    private readonly DateTimeOffset _utcNow;
    private readonly TimeZoneInfo _zone;

    public FixedTimeProvider(DateTimeOffset utcNow, TimeZoneInfo zone)
    {
        _utcNow = utcNow;
        _zone = zone;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public override TimeZoneInfo LocalTimeZone => _zone;

    /// <summary>2026-09-19 06:30:05 UTC in a fixed UTC+8 zone (Hong Kong local time 14:30:05).</summary>
    public static FixedTimeProvider HongKongAfternoon() => new(
        new DateTimeOffset(2026, 9, 19, 6, 30, 5, TimeSpan.Zero),
        TimeZoneInfo.CreateCustomTimeZone("Test+8", TimeSpan.FromHours(8), "Test+8", "Test+8"));
}
