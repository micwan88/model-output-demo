namespace TMDEmulator.Tests.Support;

/// <summary>A clock frozen at one instant, with a chosen local time zone.</summary>
public sealed class FixedTimeProvider(DateTimeOffset utcNow, TimeZoneInfo? localZone = null) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utcNow;

    public override TimeZoneInfo LocalTimeZone { get; } = localZone ?? TimeZoneInfo.Utc;
}
