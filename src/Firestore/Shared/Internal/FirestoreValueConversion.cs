using System.Globalization;

namespace Plugin.Firebase.Firestore.Internal;

/// <summary>
/// Converts a value read from Firestore to the target type of a typed read, with the same rules on
/// Android and iOS. The platforms unwrap the native value to .NET first: text to a
/// <see cref="string"/>, a boolean to a <see cref="bool"/>, a number to a numeric primitive, and
/// timestamps, geo points, references, arrays and maps to the types they read into.
/// </summary>
/// <remarks>
/// Like the native Android SDK's <c>CustomClassMapper</c>, a typed read only converts between
/// numeric types:
/// <list type="bullet">
/// <item>
/// A value that already is an instance of the target type is returned unchanged, for example text
/// read into a <see cref="string"/> or a boolean read into a <see cref="bool"/>.
/// </item>
/// <item>
/// A number read into a numeric or enum target is converted by
/// <see cref="FirestoreNumericConversion"/>, which range checks it and truncates fractions.
/// </item>
/// <item>
/// Every other pairing throws <see cref="InvalidCastException"/>: a number read into a
/// <see cref="string"/>, text into a number or an enum, a boolean into a number, a number into a
/// <see cref="bool"/>, anything into a <see cref="char"/>, and a number or text into a date or any
/// other type. A map or an array read into a type that holds a single value throws as well.
/// </item>
/// </list>
/// </remarks>
internal static class FirestoreValueConversion
{
    /// <summary>
    /// Converts <paramref name="value"/> to <paramref name="targetType"/>.
    /// </summary>
    /// <param name="value">The value read from Firestore, unwrapped to .NET.</param>
    /// <param name="targetType">
    /// The requested type, or <see langword="null"/> for an untyped read. A nullable type is converted
    /// to its underlying type.
    /// </param>
    /// <param name="field">The Firestore field the value was read from, if known.</param>
    /// <returns>
    /// The value as <paramref name="targetType"/>, or <see langword="null"/> if
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Thrown if the type of <paramref name="value"/> doesn't match <paramref name="targetType"/>.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Thrown if a number can't be represented as <paramref name="targetType"/>.
    /// </exception>
    public static object? ToTargetType(object? value, Type? targetType, string? field = null)
    {
        if(value == null || targetType == null) {
            return value;
        }

        var conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if(conversionType.IsInstanceOfType(value)) {
            return value;
        }

        if(FirestoreNumericConversion.TryConvert(value, conversionType, out var converted)) {
            return converted;
        }

        throw Mismatch(
            string.Format(CultureInfo.InvariantCulture, "a value of type {0}", value.GetType()),
            conversionType,
            field);
    }

    /// <summary>
    /// Throws if a map can't be read into <paramref name="targetType"/> because the type holds a
    /// single value.
    /// </summary>
    /// <param name="targetType">The requested type, or <see langword="null"/> for an untyped read.</param>
    /// <param name="field">The Firestore field the map was read from, if known.</param>
    /// <exception cref="InvalidCastException">
    /// Thrown if <paramref name="targetType"/> is a <see cref="string"/>, <see cref="bool"/>,
    /// <see cref="char"/>, numeric, enum, <see cref="DateTime"/> or <see cref="DateTimeOffset"/> type.
    /// </exception>
    public static void EnsureMapTarget(Type? targetType, string? field = null)
    {
        EnsureCollectionTarget("a map", targetType, field);
    }

    /// <summary>
    /// Throws if an array can't be read into <paramref name="targetType"/> because the type holds a
    /// single value.
    /// </summary>
    /// <param name="targetType">The requested type, or <see langword="null"/> for an untyped read.</param>
    /// <param name="field">The Firestore field the array was read from, if known.</param>
    /// <exception cref="InvalidCastException">
    /// Thrown if <paramref name="targetType"/> is a <see cref="string"/>, <see cref="bool"/>,
    /// <see cref="char"/>, numeric, enum, <see cref="DateTime"/> or <see cref="DateTimeOffset"/> type.
    /// </exception>
    public static void EnsureArrayTarget(Type? targetType, string? field = null)
    {
        EnsureCollectionTarget("an array", targetType, field);
    }

    private static void EnsureCollectionTarget(string source, Type? targetType, string? field)
    {
        if(targetType == null) {
            return;
        }

        // Without this check a map read into one of these types would come back as its default value.
        // Every type with a type code other than Object holds a single value: string, bool, char,
        // the numeric types, enums and DateTime.
        var conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if(Type.GetTypeCode(conversionType) != TypeCode.Object || conversionType == typeof(DateTimeOffset)) {
            throw Mismatch(source, conversionType, field);
        }
    }

    private static InvalidCastException Mismatch(string source, Type conversionType, string? field)
    {
        var origin = field == null
            ? "Firestore"
            : string.Format(CultureInfo.InvariantCulture, "Firestore field '{0}'", field);
        return new InvalidCastException(
            string.Format(
                CultureInfo.InvariantCulture,
                "Can't convert {0} read from {1} to {2}.",
                source,
                origin,
                conversionType));
    }
}