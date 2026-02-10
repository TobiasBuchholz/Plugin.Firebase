using Firebase.Core;

namespace Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

/// <summary>
/// Extension methods for converting Firestore timestamp types to .NET date types.
/// </summary>
public static class DateExtensions
{
    /// <summary>
    /// Converts a native iOS Firestore timestamp to a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="this">The native timestamp to convert.</param>
    /// <returns>A <see cref="DateTimeOffset"/> representation of the timestamp, or default if null.</returns>
    public static DateTimeOffset ToDateTimeOffset(this Timestamp @this)
    {
        return @this == null
            ? default(DateTimeOffset)
            : DateTime.SpecifyKind((DateTime) @this.DateValue, DateTimeKind.Utc);
    }

    /// <summary>
    /// Converts a native iOS Firestore timestamp to a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="this">The native timestamp to convert.</param>
    /// <returns>A <see cref="DateTime"/> representation of the timestamp in UTC, or default if null.</returns>
    public static DateTime ToDateTime(this Timestamp @this)
    {
        return @this == null
            ? default(DateTime)
            : DateTime.SpecifyKind((DateTime) @this.DateValue, DateTimeKind.Utc);
    }
}