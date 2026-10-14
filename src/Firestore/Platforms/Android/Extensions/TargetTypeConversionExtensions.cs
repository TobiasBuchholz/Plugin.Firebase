using System.Globalization;
using Plugin.Firebase.Firestore.Internal;

namespace Plugin.Firebase.Firestore.Platforms.Android.Extensions;

internal static class TargetTypeConversionExtensions
{
    public static Type? GetConversionType(this Type? targetType)
    {
        return targetType == null
            ? null
            : Nullable.GetUnderlyingType(targetType) ?? targetType;
    }

    public static object? ConvertToTargetType(this object? value, Type? targetType)
    {
        var conversionType = targetType.GetConversionType();
        if(value == null || conversionType == null || conversionType == typeof(object) || conversionType.IsInstanceOfType(value)) {
            return value;
        }

        // numbers read into numeric and enum targets follow the rules shared with iOS: truncation and range checks
        if(FirestoreNumericConversion.TryConvert(value, conversionType, out var converted)) {
            return converted;
        }

        if(conversionType.IsEnum) {
            return Enum.ToObject(conversionType, value);
        }

        // the invariant culture keeps string <-> number conversions independent of the device locale
        return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
    }
}
