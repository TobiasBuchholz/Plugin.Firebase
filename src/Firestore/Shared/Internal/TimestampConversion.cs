namespace Plugin.Firebase.Firestore.Internal;

/// <summary>
/// Converts between .NET dates and the seconds/nanoseconds pair of a native Firestore <c>Timestamp</c>.
/// </summary>
/// <remarks>
/// Platform-neutral so the arithmetic can be unit tested without a device. A .NET tick is 100 ns, so the
/// conversions are exact at 100 ns granularity. The native SDKs then truncate written values to the
/// microseconds that Firestore stores.
/// </remarks>
internal static class TimestampConversion
{
    private const long NanosecondsPerTick = 100;

    private static readonly long UnixEpochTicks = DateTimeOffset.UnixEpoch.UtcTicks;

    /// <summary>
    /// Splits the UTC instant of <paramref name="value"/> into whole seconds since the Unix epoch and the
    /// nanoseconds within that second. The offset of <paramref name="value"/> does not change the result.
    /// </summary>
    /// <returns>
    /// The seconds, rounded toward negative infinity so instants before the epoch stay correct, and the
    /// nanoseconds, always between 0 and 999,999,900. This is the form the native <c>Timestamp</c> requires.
    /// </returns>
    public static (long Seconds, int Nanoseconds) ToUnixSecondsAndNanoseconds(DateTimeOffset value)
    {
        var (seconds, remainderTicks) = Math.DivRem(value.UtcTicks - UnixEpochTicks, TimeSpan.TicksPerSecond);
        if(remainderTicks < 0) {
            seconds -= 1;
            remainderTicks += TimeSpan.TicksPerSecond;
        }
        return (seconds, (int) (remainderTicks * NanosecondsPerTick));
    }

    /// <summary>
    /// Splits the instant of <paramref name="value"/> into whole seconds since the Unix epoch and the
    /// nanoseconds within that second, as <see cref="ToUnixSecondsAndNanoseconds(DateTimeOffset)"/> does.
    /// </summary>
    /// <remarks>
    /// <see cref="DateTimeKind.Local"/> and <see cref="DateTimeKind.Utc"/> values convert through their UTC
    /// instant. <see cref="DateTimeKind.Unspecified"/> values are read as local time, as the implicit
    /// <see cref="DateTime"/> to <see cref="DateTimeOffset"/> conversion does.
    /// </remarks>
    public static (long Seconds, int Nanoseconds) ToUnixSecondsAndNanoseconds(DateTime value)
    {
        return ToUnixSecondsAndNanoseconds(new DateTimeOffset(value));
    }

    /// <summary>
    /// Builds the UTC <see cref="DateTimeOffset"/> for whole seconds since the Unix epoch plus nanoseconds,
    /// truncating the nanoseconds to whole 100 ns ticks.
    /// </summary>
    /// <param name="seconds">Seconds since the Unix epoch.</param>
    /// <param name="nanoseconds">Nanoseconds within the second, between 0 and 999,999,999 as the native
    /// <c>Timestamp</c> guarantees.</param>
    /// <returns>The instant with a zero offset.</returns>
    public static DateTimeOffset ToDateTimeOffset(long seconds, int nanoseconds)
    {
        var ticks = checked(UnixEpochTicks + seconds * TimeSpan.TicksPerSecond + nanoseconds / NanosecondsPerTick);
        return new DateTimeOffset(ticks, TimeSpan.Zero);
    }
}