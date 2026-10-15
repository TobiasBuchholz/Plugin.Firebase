using System.Collections;
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
/// numeric types. Unlike it, an enum is read from a number rather than from a member name, because
/// the plugin writes enums as numbers:
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
/// other type. A map only reads into <see cref="object"/>, a dictionary or a model, and an array
/// only into <see cref="object"/> or a list; any other target throws as well.
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
    /// Converts a map key, or a field name when a document is read as a dictionary, to
    /// <paramref name="keyType"/>. Firestore always stores keys as text.
    /// </summary>
    /// <param name="key">The key read from Firestore.</param>
    /// <param name="keyType">The requested key type.</param>
    /// <returns>The key, if it is an instance of <paramref name="keyType"/>.</returns>
    /// <exception cref="InvalidCastException">
    /// Thrown if <paramref name="keyType"/> can't hold a <see cref="string"/>.
    /// </exception>
    public static object ToKeyType(string key, Type keyType)
    {
        var conversionType = Nullable.GetUnderlyingType(keyType) ?? keyType;
        if(conversionType.IsInstanceOfType(key)) {
            return key;
        }

        throw Mismatch(
            string.Format(CultureInfo.InvariantCulture, "the map key '{0}'", key),
            conversionType,
            field: null);
    }

    /// <summary>
    /// Throws if a map can't be read into <paramref name="targetType"/>. A map reads into
    /// <see cref="object"/>, an <c>IDictionary&lt;TKey, TValue&gt;</c> or
    /// <c>Dictionary&lt;TKey, TValue&gt;</c>, or a model: a concrete class, or a struct that
    /// implements <see cref="IFirestoreObject"/>.
    /// </summary>
    /// <param name="targetType">The requested type, or <see langword="null"/> for an untyped read.</param>
    /// <param name="field">The Firestore field the map was read from, if known.</param>
    /// <exception cref="InvalidCastException">
    /// Thrown for any other <paramref name="targetType"/>, for example a <see cref="string"/>, a
    /// number, a list or a <see cref="GeoPoint"/>.
    /// </exception>
    public static void EnsureMapTarget(Type? targetType, string? field = null)
    {
        if(targetType == null) {
            return;
        }

        var conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if(!CanHoldMap(conversionType)) {
            throw Mismatch("a map", conversionType, field);
        }
    }

    /// <summary>
    /// Throws if an array can't be read into <paramref name="targetType"/>. An array reads into
    /// <see cref="object"/>, or a type that a <see cref="List{T}"/> can be assigned to, such as
    /// <see cref="IList{T}"/>.
    /// </summary>
    /// <param name="targetType">The requested type, or <see langword="null"/> for an untyped read.</param>
    /// <param name="field">The Firestore field the array was read from, if known.</param>
    /// <exception cref="InvalidCastException">
    /// Thrown for any other <paramref name="targetType"/>, for example a <see cref="string"/>, a
    /// number, a dictionary or a model. An array target such as <c>string[]</c> passes, so that the
    /// platform can say that lists are supported instead.
    /// </exception>
    public static void EnsureArrayTarget(Type? targetType, string? field = null)
    {
        if(targetType == null) {
            return;
        }

        var conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if(!CanHoldArray(conversionType)) {
            throw Mismatch("an array", conversionType, field);
        }
    }

    private static bool CanHoldMap(Type conversionType)
    {
        if(conversionType == typeof(object) || IsDictionaryType(conversionType)) {
            return true;
        }

        // The platforms create a model with Activator.CreateInstance and fill its properties. An
        // interface or abstract class can't be created, and a collection, a string, or a value such
        // as an int or a GeoPoint would come back empty or fail. A struct is only a model if it says so.
        if(conversionType.IsAbstract
           || conversionType == typeof(GeoPoint)
           || typeof(IEnumerable).IsAssignableFrom(conversionType)) {
            return false;
        }
        return conversionType.IsClass || typeof(IFirestoreObject).IsAssignableFrom(conversionType);
    }

    private static bool CanHoldArray(Type conversionType)
    {
        if(conversionType == typeof(object) || conversionType.IsArray) {
            return true;
        }

        // The platforms read an array into a List<T> of the target's only type argument.
        var arguments = conversionType.GenericTypeArguments;
        return arguments.Length == 1
               && conversionType.IsAssignableFrom(typeof(List<>).MakeGenericType(arguments[0]));
    }

    private static bool IsDictionaryType(Type conversionType)
    {
        // The same dictionary types the platforms read a map into.
        return conversionType.IsGenericType
               && (conversionType.GetGenericTypeDefinition() == typeof(IDictionary<,>)
                   || conversionType.GetGenericTypeDefinition() == typeof(Dictionary<,>));
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