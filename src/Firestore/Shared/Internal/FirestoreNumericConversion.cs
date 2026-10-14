using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Plugin.Firebase.Firestore.Internal;

/// <summary>
/// Converts numbers read from Firestore into numeric and enum target types, with the same rules on
/// Android and iOS. The platforms unwrap the native value to a .NET primitive first.
/// </summary>
/// <remarks>
/// The rules follow the native Android SDK's <c>CustomClassMapper</c>:
/// <list type="bullet">
/// <item>
/// A value outside the target's range throws <see cref="OverflowException"/>. So does NaN or
/// infinity read into an integral, enum or <see cref="decimal"/> target, and a finite value too
/// large for a <see cref="float"/> target.
/// </item>
/// <item>
/// A fractional value read into an integral or enum target is range checked as stored, then
/// truncated toward zero. So 2.7 gives 2, but 255.5 doesn't fit a <see cref="byte"/>.
/// </item>
/// <item>
/// An enum is converted through its underlying type, so a value that fits that type but isn't a
/// named member still converts, as with a C# cast.
/// </item>
/// </list>
/// </remarks>
internal static class FirestoreNumericConversion
{
    /// <summary>
    /// Converts <paramref name="value"/> to <paramref name="targetType"/> when the value is a number and
    /// the target is a numeric or enum type.
    /// </summary>
    /// <param name="value">The value read from Firestore, unwrapped to a .NET primitive.</param>
    /// <param name="targetType">The requested type. A nullable type is converted to its underlying type.</param>
    /// <param name="result">The converted value, boxed as the target type or its underlying type.</param>
    /// <returns>
    /// <see langword="true"/> if the value was converted; <see langword="false"/> if these rules don't
    /// cover the value and target, which leaves the conversion to the caller.
    /// </returns>
    /// <exception cref="OverflowException">
    /// Thrown if the value can't be represented as <paramref name="targetType"/>.
    /// </exception>
    public static bool TryConvert(object value, Type targetType, [NotNullWhen(true)] out object? result)
    {
        var conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // Only numbers read into numeric or enum targets are converted here. Rules for any other
        // pairing, such as text read into a number or a number read into text, belong at this
        // point; until then the caller handles them.
        if(!IsNumber(value) || !IsNumericTarget(conversionType)) {
            result = null;
            return false;
        }

        if(conversionType.IsEnum) {
            // Enum.ToObject narrows without a range check, so convert to the underlying type first.
            var underlying = ConvertNumber(value, Enum.GetUnderlyingType(conversionType), conversionType);
            result = Enum.ToObject(conversionType, underlying);
        } else {
            result = ConvertNumber(value, conversionType, conversionType);
        }
        return true;
    }

    private static bool IsNumber(object value)
    {
        return value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }

    private static bool IsNumericTarget(Type conversionType)
    {
        // An enum reports the type code of its underlying type. SByte through UInt64 are the
        // integral type codes, followed by Single, Double and Decimal.
        var typeCode = Type.GetTypeCode(conversionType);
        return conversionType.IsEnum
            ? typeCode is >= TypeCode.SByte and <= TypeCode.UInt64
            : typeCode is >= TypeCode.SByte and <= TypeCode.Decimal;
    }

    private static object ConvertNumber(object value, Type conversionType, Type targetType)
    {
        try {
            var typeCode = Type.GetTypeCode(conversionType);
            return typeCode switch {
                TypeCode.Double => Convert.ToDouble(value, CultureInfo.InvariantCulture),
                TypeCode.Single => ToSingle(value),
                TypeCode.Decimal => Convert.ToDecimal(RequireFinite(value), CultureInfo.InvariantCulture),
                _ => ToIntegral(value, conversionType, typeCode)
            };
        } catch(OverflowException exception) {
            throw new OverflowException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "The number {0} read from Firestore can't be represented as {1}.",
                    value,
                    targetType),
                exception);
        }
    }

    private static float ToSingle(object value)
    {
        var number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
        var single = (float) number;
        // A float holds NaN and infinity as well, so only a finite value that became infinite overflowed.
        if(float.IsInfinity(single) && double.IsFinite(number)) {
            throw new OverflowException();
        }
        return single;
    }

    private static object RequireFinite(object value)
    {
        var isFinite = value switch {
            double d => double.IsFinite(d),
            float f => float.IsFinite(f),
            _ => true
        };
        return isFinite ? value : throw new OverflowException();
    }

    private static object ToIntegral(object value, Type conversionType, TypeCode typeCode)
    {
        // Like the native Android SDK, a fractional source is range checked as stored and only then
        // truncated, so 2147483647.9 doesn't fit an int. After truncating, Convert has nothing left
        // to round; it range checks integral sources itself.
        object integral = value switch {
            double d => FitsIntegral(d, typeCode) ? Math.Truncate(d) : throw new OverflowException(),
            float f => FitsIntegral(f, typeCode) ? Math.Truncate((double) f) : throw new OverflowException(),
            decimal m => FitsIntegral(m, typeCode) ? decimal.Truncate(m) : throw new OverflowException(),
            _ => value
        };
        return Convert.ChangeType(integral, conversionType, CultureInfo.InvariantCulture)!;
    }

    private static bool FitsIntegral(double value, TypeCode typeCode)
    {
        // NaN fails every comparison and infinity is outside every range, so neither fits.
        return typeCode switch {
            TypeCode.SByte => value >= sbyte.MinValue && value <= sbyte.MaxValue,
            TypeCode.Byte => value >= byte.MinValue && value <= byte.MaxValue,
            TypeCode.Int16 => value >= short.MinValue && value <= short.MaxValue,
            TypeCode.UInt16 => value >= ushort.MinValue && value <= ushort.MaxValue,
            TypeCode.Int32 => value >= int.MinValue && value <= int.MaxValue,
            TypeCode.UInt32 => value >= uint.MinValue && value <= uint.MaxValue,
            // long.MaxValue and ulong.MaxValue round up to 2^63 and 2^64 as doubles, so the upper
            // bounds are those powers of two, compared strictly.
            TypeCode.Int64 => value >= -9223372036854775808.0 && value < 9223372036854775808.0,
            TypeCode.UInt64 => value >= 0.0 && value < 18446744073709551616.0,
            _ => false
        };
    }

    private static bool FitsIntegral(decimal value, TypeCode typeCode)
    {
        return typeCode switch {
            TypeCode.SByte => value >= sbyte.MinValue && value <= sbyte.MaxValue,
            TypeCode.Byte => value >= byte.MinValue && value <= byte.MaxValue,
            TypeCode.Int16 => value >= short.MinValue && value <= short.MaxValue,
            TypeCode.UInt16 => value >= ushort.MinValue && value <= ushort.MaxValue,
            TypeCode.Int32 => value >= int.MinValue && value <= int.MaxValue,
            TypeCode.UInt32 => value >= uint.MinValue && value <= uint.MaxValue,
            TypeCode.Int64 => value >= long.MinValue && value <= long.MaxValue,
            TypeCode.UInt64 => value >= ulong.MinValue && value <= ulong.MaxValue,
            _ => false
        };
    }
}