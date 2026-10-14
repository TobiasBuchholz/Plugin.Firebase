using System.Globalization;

namespace Plugin.Firebase.Firestore.Internal;

/// <summary>
/// Rejects <see cref="DateTime"/> values that don't say which instant they are before Firestore stores them.
/// </summary>
/// <remarks>
/// A Firestore timestamp is an instant. A <see cref="DateTime"/> with <see cref="DateTimeKind.Unspecified"/> could be
/// UTC or local time, and the two give different instants unless the device runs in UTC. Every Firestore write of a
/// <see cref="DateTime"/> on Android and iOS, including query operands and cursor values, calls this guard rather than
/// assume one of them.
/// </remarks>
internal static class DateTimeKindGuard
{
    /// <summary>
    /// Throws when <paramref name="value"/> has <see cref="DateTimeKind.Unspecified"/>.
    /// <see cref="DateTimeKind.Utc"/> and <see cref="DateTimeKind.Local"/> values pass.
    /// </summary>
    /// <param name="value">The value about to be converted for the native Firestore SDK.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> has <see cref="DateTimeKind.Unspecified"/>.</exception>
    public static void ThrowIfUnspecified(DateTime value)
    {
        if(value.Kind == DateTimeKind.Unspecified) {
            throw new ArgumentException(
                $"Firestore can't store the DateTime {value.ToString("O", CultureInfo.InvariantCulture)} because its Kind is "
                + "DateTimeKind.Unspecified, so it could be UTC or local time. Call DateTime.SpecifyKind(value, DateTimeKind.Utc) "
                + "or DateTime.SpecifyKind(value, DateTimeKind.Local) to say which, or use a DateTimeOffset.",
                nameof(value));
        }
    }
}