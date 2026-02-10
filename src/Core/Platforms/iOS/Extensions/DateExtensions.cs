namespace Plugin.Firebase.Core.Platforms.iOS.Extensions;

/// <summary>
/// Extension methods for converting between .NET date types and iOS NSDate.
/// </summary>
public static class DateExtensions
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> to an <see cref="NSDate"/>.
    /// </summary>
    /// <param name="this">The DateTime to convert.</param>
    /// <returns>An NSDate representing the same point in time.</returns>
    public static NSDate ToNSDate(this DateTime @this)
    {
        if(@this.Kind == DateTimeKind.Unspecified) {
            @this = DateTime.SpecifyKind(@this, DateTimeKind.Utc);
        }
        return (NSDate) @this;
    }

    /// <summary>
    /// Converts a <see cref="DateTimeOffset"/> to an <see cref="NSDate"/>.
    /// </summary>
    /// <param name="this">The DateTimeOffset to convert.</param>
    /// <returns>An NSDate representing the same point in time.</returns>
    public static NSDate ToNSDate(this DateTimeOffset @this)
    {
        return @this.DateTime.ToNSDate();
    }

    /// <summary>
    /// Converts an <see cref="NSDate"/> to a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="this">The NSDate to convert.</param>
    /// <returns>A DateTimeOffset representing the same point in time, or default if null.</returns>
    public static DateTimeOffset ToDateTimeOffset(this NSDate @this)
    {
        return @this == null
            ? default(DateTimeOffset)
            : DateTime.SpecifyKind((DateTime) @this, DateTimeKind.Utc);
    }
}