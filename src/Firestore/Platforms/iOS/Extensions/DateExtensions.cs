using Firebase.Core;
using Plugin.Firebase.Firestore.Internal;

namespace Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

/// <summary>
/// Extension methods for converting Firestore timestamp types to .NET date types.
/// </summary>
/// <remarks>
/// The conversions use the timestamp's seconds and nanoseconds since the Unix epoch, as on Android. They don't go
/// through <see cref="NSDate"/>, whose conversion to and from <see cref="DateTime"/> uses a calendar that is Julian
/// before 1582-10-15 and keeps less precision.
/// </remarks>
public static class DateExtensions
{
    /// <summary>
    /// Converts a native iOS Firestore timestamp to a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="this">The native timestamp to convert.</param>
    /// <returns>The exact instant of the timestamp, truncated to 100 ns ticks, with a zero offset, or default if
    /// null.</returns>
    public static DateTimeOffset ToDateTimeOffset(this Timestamp @this)
    {
        return @this == null
            ? default(DateTimeOffset)
            : TimestampConversion.ToDateTimeOffset(@this.Seconds, @this.Nanoseconds);
    }

    /// <summary>
    /// Converts a native iOS Firestore timestamp to a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="this">The native timestamp to convert.</param>
    /// <returns>The exact instant of the timestamp, truncated to 100 ns ticks, as a <see cref="DateTimeKind.Utc"/>
    /// <see cref="DateTime"/>, or default if null.</returns>
    public static DateTime ToDateTime(this Timestamp @this)
    {
        return @this == null
            ? default(DateTime)
            : @this.ToDateTimeOffset().UtcDateTime;
    }

    /// <summary>
    /// Creates a native Firestore timestamp for the exact UTC instant of <paramref name="this"/>.
    /// </summary>
    internal static Timestamp ToTimestamp(this DateTimeOffset @this)
    {
        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(@this);
        return new Timestamp(seconds, nanoseconds);
    }

    /// <summary>
    /// Creates a native Firestore timestamp for the exact UTC instant of <paramref name="this"/>. Every Firestore
    /// write of a <see cref="DateTime"/> on iOS goes through this method.
    /// </summary>
    /// <remarks>
    /// <see cref="DateTimeKind.Local"/> and <see cref="DateTimeKind.Utc"/> values convert through their UTC
    /// instant. <see cref="DateTimeKind.Unspecified"/> values throw, as they do on Android, because they could be
    /// UTC or local time.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="this"/> has <see cref="DateTimeKind.Unspecified"/>.</exception>
    internal static Timestamp ToTimestamp(this DateTime @this)
    {
        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(@this);
        return new Timestamp(seconds, nanoseconds);
    }
}