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
            var underlying = ChangeTypeChecked(value, underlyingType);
            return Enum.ToObject(conversionType, underlying);
        }

        // the invariant culture keeps string <-> number conversions independent of the device locale
        return ChangeTypeChecked(value, conversionType);
    }

    /// <summary>
    /// Narrows a value through <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/>,
    /// rejecting fractional floating point sources for integral targets instead of rounding
    /// them silently. Values like 2.0 are fine since nothing is lost.
    /// </summary>
    private static object ChangeTypeChecked(object value, Type conversionType)
    {
        var hasFraction = value switch {
            double d => d != Math.Truncate(d),
            float f => f != Math.Truncate(f),
            decimal m => m != decimal.Truncate(m),
            _ => false
        };
        if(hasFraction && IsIntegralType(conversionType)) {
            throw new InvalidCastException(
                $"Cannot convert a fractional {value.GetType().Name} to integral type {conversionType.Name}"
            );
        }
        return Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
    }

    private static bool IsIntegralType(Type type)
    {
        return Type.GetTypeCode(type) switch {
            TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16
                or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 => true,
            _ => false
        };
    }
}