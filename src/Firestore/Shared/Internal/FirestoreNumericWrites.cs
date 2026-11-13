using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace Plugin.Firebase.Firestore.Internal;

/// <summary>
/// Decides which CLR numeric types a Firestore write accepts and what each one is stored as. Firestore stores every
/// number as a signed 64-bit integer or a double, so integral values widen to <see cref="long"/> and
/// <see cref="float"/> widens to <see cref="double"/>. Types that have no lossless Firestore representation throw
/// instead of being converted silently. Both platforms pass every written value through this mapping first, so
/// set, update, query, list and array-union writes store numbers the same way. <see cref="byte"/> and
/// <see cref="sbyte"/> arrays are rejected here too, because the platforms would otherwise write them element by
/// element as an int64 array rather than as Firestore bytes.
/// </summary>
internal static class FirestoreNumericWrites
{
    /// <summary>
    /// Widens a CLR number or enum to the value Firestore stores.
    /// </summary>
    /// <param name="value">The value being written.</param>
    /// <param name="widened">A boxed <see cref="long"/> or <see cref="double"/> when this method returns true.</param>
    /// <returns>True if <paramref name="value"/> is a number or an enum; false for any other value, including null.</returns>
    /// <remarks>
    /// Unlike most <c>Try</c> methods, this one throws: false only means that <paramref name="value"/> isn't a number
    /// and the platform converts it another way, while a value Firestore can't store fails the write here.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is a <see cref="decimal"/>, <see cref="char"/>, <see cref="nint"/>, <see cref="nuint"/>,
    /// <see cref="Half"/>, <see cref="Int128"/>, <see cref="UInt128"/> or <see cref="BigInteger"/>, a
    /// <see cref="ulong"/> or ulong-backed enum value above <see cref="long.MaxValue"/>, or a <see cref="byte"/> or
    /// <see cref="sbyte"/> array.
    /// </exception>
    public static bool TryWiden(object? value, [NotNullWhen(true)] out object? widened)
    {
        // an enum is written as its underlying value, under the same rules as that integral type
        var number = value is Enum
            ? Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()), CultureInfo.InvariantCulture)
            : value;

        switch(number) {
            case sbyte or byte or short or ushort or int or uint or long:
                widened = Convert.ToInt64(number, CultureInfo.InvariantCulture);
                return true;
            case ulong x when x <= long.MaxValue:
                widened = (long) x;
                return true;
            case ulong x:
                throw new ArgumentException(string.Create(
                    CultureInfo.InvariantCulture,
                    $"Firestore integers are signed 64-bit, so the {value!.GetType()} value {x} can't be written. Values above {long.MaxValue} have no Firestore representation."));
            case float x:
                widened = (double) x;
                return true;
            case double x:
                widened = x;
                return true;
            case decimal:
                throw new ArgumentException(
                    $"Firestore has no decimal type, so {value!.GetType()} values can't be written. Convert the value to double explicitly if losing precision is acceptable.");
            case char:
                throw new ArgumentException(
                    $"Firestore has no character type, so {value!.GetType()} values can't be written. Convert the value to a string or an integral type explicitly.");
            case nint or nuint or Half or Int128 or UInt128 or BigInteger:
                throw new ArgumentException(
                    $"Firestore numbers are 64-bit integers or doubles, so {value!.GetType()} values can't be written. Convert the value to long or double explicitly.");
            case byte[] or sbyte[]:
                throw new ArgumentException(
                    $"Firestore bytes (Blob) values aren't supported by this API, so {value!.GetType()} values can't be written. Convert the value explicitly, for example to a Base64 string.");
            default:
                widened = null;
                return false;
        }
    }
}