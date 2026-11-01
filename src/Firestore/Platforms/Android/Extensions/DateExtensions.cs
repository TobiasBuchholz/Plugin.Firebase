using Java.Util;
using Plugin.Firebase.Firestore.Internal;
using Timestamp = global::Firebase.Timestamp;

namespace Plugin.Firebase.Firestore.Platforms.Android.Extensions;

public static class DateExtensions
{
    public static DateTimeOffset ToDateTimeOffset(this Date date)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(date.Time);
    }

    public static DateTime ToDateTime(this Date date)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(date.Time).UtcDateTime;
    }

    public static Date ToJavaDate(this DateTimeOffset @this)
    {
        return new Date(@this.ToUnixTimeMilliseconds());
    }

    public static Date ToJavaDate(this DateTime @this)
    {
        return ToJavaDate((DateTimeOffset) @this);
    }

    /// <summary>
    /// Converts a native Firestore timestamp to a UTC <see cref="DateTimeOffset"/>, keeping its sub-millisecond
    /// precision down to 100 ns, unlike <see cref="Timestamp.ToDate"/> which truncates to milliseconds.
    /// </summary>
    internal static DateTimeOffset ToDateTimeOffset(this Timestamp timestamp)
    {
        return TimestampConversion.ToDateTimeOffset(timestamp.Seconds, timestamp.Nanoseconds);
    }

    /// <summary>
    /// Converts a native Firestore timestamp to a UTC <see cref="DateTime"/>, keeping its sub-millisecond
    /// precision down to 100 ns, unlike <see cref="Timestamp.ToDate"/> which truncates to milliseconds.
    /// </summary>
    internal static DateTime ToDateTime(this Timestamp timestamp)
    {
        return timestamp.ToDateTimeOffset().UtcDateTime;
    }

    /// <summary>
    /// Creates a native Firestore timestamp for the UTC instant of <paramref name="this"/>, keeping the
    /// sub-millisecond precision that <see cref="ToJavaDate(DateTimeOffset)"/> loses.
    /// </summary>
    internal static Timestamp ToTimestamp(this DateTimeOffset @this)
    {
        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(@this);
        return new Timestamp(seconds, nanoseconds);
    }

    /// <summary>
    /// Creates a native Firestore timestamp for <paramref name="this"/>, keeping the sub-millisecond precision
    /// that <see cref="ToJavaDate(DateTime)"/> loses. Every Firestore write of a <see cref="DateTime"/> on
    /// Android goes through this method.
    /// </summary>
    /// <remarks>
    /// <see cref="DateTimeKind.Local"/> and <see cref="DateTimeKind.Utc"/> values convert through their UTC
    /// instant. <see cref="DateTimeKind.Unspecified"/> values are read as local time.
    /// </remarks>
    internal static Timestamp ToTimestamp(this DateTime @this)
    {
        var (seconds, nanoseconds) = TimestampConversion.ToUnixSecondsAndNanoseconds(@this);
        return new Timestamp(seconds, nanoseconds);
    }
}