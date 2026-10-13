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
            // converting through the underlying type lets ChangeType throw OverflowException for
            // stored values that don't fit the enum, instead of Enum.ToObject silently wrapping
            var underlyingType = Enum.GetUnderlyingType(conversionType);
            var underlying = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
            return Enum.ToObject(conversionType, underlying);
        }

        // the invariant culture keeps string <-> number conversions independent of the device locale
        return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
    }
}