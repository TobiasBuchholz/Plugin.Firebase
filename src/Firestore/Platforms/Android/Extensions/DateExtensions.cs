using Java.Util;
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
    /// Converts a native Firestore timestamp without losing its sub-millisecond precision, unlike
    /// <see cref="Timestamp.ToDate"/> which rounds to milliseconds.
    /// </summary>
    public static DateTimeOffset ToDateTimeOffset(this Timestamp timestamp)
    {
        return DateTimeOffset.UnixEpoch
            .AddTicks(timestamp.Seconds * TimeSpan.TicksPerSecond + timestamp.Nanoseconds / 100);
    }

    /// <summary>
    /// Converts a native Firestore timestamp without losing its sub-millisecond precision, unlike
    /// <see cref="Timestamp.ToDate"/> which rounds to milliseconds.
    /// </summary>
    public static DateTime ToDateTime(this Timestamp timestamp)
    {
        return timestamp.ToDateTimeOffset().UtcDateTime;
    }

    /// <summary>
    /// Creates a native Firestore timestamp, preserving sub-millisecond precision that
    /// <see cref="ToJavaDate(DateTimeOffset)"/> would lose.
    /// </summary>
    public static Timestamp ToTimestamp(this DateTimeOffset @this)
    {
        var seconds = @this.ToUnixTimeSeconds();
        var nanos =
            (int) (@this.Ticks - (DateTimeOffset.UnixEpoch.Ticks + seconds * TimeSpan.TicksPerSecond))
            * 100;
        if(nanos < 0) {
            seconds -= 1;
            nanos += 1_000_000_000;
        }
        return new Timestamp(seconds, nanos);
    }

    /// <summary>
    /// Creates a native Firestore timestamp, preserving sub-millisecond precision that
    /// <see cref="ToJavaDate(DateTime)"/> would lose.
    /// </summary>
    public static Timestamp ToTimestamp(this DateTime @this)
    {
        return ToTimestamp((DateTimeOffset) @this);
    }
}