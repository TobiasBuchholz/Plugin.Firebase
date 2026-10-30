using System.Globalization;

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

        if(conversionType.IsEnum) {
            // Enum.ToObject only accepts integral values, so values stored as doubles go through the underlying type first
            if(value is double or float or decimal) {
                value = Convert.ChangeType(value, Enum.GetUnderlyingType(conversionType), CultureInfo.InvariantCulture);
            }
            return Enum.ToObject(conversionType, value);
        }

        // the invariant culture keeps string <-> number conversions independent of the device locale
        return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
    }
}
