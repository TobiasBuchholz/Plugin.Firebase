using System.Globalization;

namespace Plugin.Firebase.Auth.Internal;

/// <summary>
/// Converts a scalar claim value, already read from the native token into its natural .NET type, into the type
/// requested through <see cref="IAuthTokenResult.GetClaim{T}(string)"/>.
/// </summary>
/// <remarks>
/// A fractional value read into an integral or enum type truncates toward zero, and a value that doesn't fit the
/// target throws <see cref="OverflowException"/>; NaN and infinity only fit <see cref="float"/> and
/// <see cref="double"/>. Strings and booleans are never coerced into numbers.
/// </remarks>
internal static class ClaimValueConverter
{
    /// <summary>
    /// Converts <paramref name="value"/> to <paramref name="targetType"/> when that type, or the type it makes
    /// nullable, is numeric, an enum or <see cref="char"/>. For any other target the value is returned unchanged and
    /// the caller's cast decides whether it fits.
    /// </summary>
    /// <param name="value">The claim value: a string, a bool, or a CLR integral or floating-point number.</param>
    /// <param name="targetType">The requested type, or null to keep the natural type.</param>
    /// <returns>The converted value, or <paramref name="value"/> itself for a non-numeric target.</returns>
    /// <exception cref="OverflowException">
    /// The value doesn't fit the numeric or enum target, or it is NaN or infinite and the target is integral, an enum
    /// or <see cref="decimal"/>.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// The value isn't a number but the target is numeric or an enum, or the target is <see cref="char"/>.
    /// </exception>
    public static object ToRequestedType(object value, Type? targetType)
    {
        if(targetType is null) {
            return value;
        }

        var conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if(conversionType.IsEnum) {
            // range check against the underlying type, so Enum.ToObject never sees a value it would wrap
            var number = ToNumber(value, Enum.GetUnderlyingType(conversionType), targetType);
            return Enum.ToObject(conversionType, number);
        }

        return IsNumeric(conversionType) ? ToNumber(value, conversionType, targetType) : value;
    }

    private static bool IsNumeric(Type type)
    {
        switch(Type.GetTypeCode(type)) {
            case TypeCode.Char:
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                return true;
            default:
                return false;
        }
    }

    private static object ToNumber(object value, Type numberType, Type targetType)
    {
        var typeCode = Type.GetTypeCode(numberType);
        if(typeCode == TypeCode.Char) {
            throw new InvalidCastException($"Claims can't be read as {targetType}.");
        }

        try {
            switch(value) {
                case sbyte or short or int or long:
                    return FromInt64(Convert.ToInt64(value, CultureInfo.InvariantCulture), typeCode);
                case byte or ushort or uint or ulong:
                    return FromUInt64(Convert.ToUInt64(value, CultureInfo.InvariantCulture), typeCode);
                case float or double:
                    return FromDouble(Convert.ToDouble(value, CultureInfo.InvariantCulture), typeCode);
                default:
                    throw new InvalidCastException(
                        $"Can't read a claim of type {value.GetType()} as {targetType}."
                    );
            }
        } catch(OverflowException e) {
            throw new OverflowException(
                FormattableString.Invariant($"The claim value {value} doesn't fit into {targetType}."),
                e
            );
        }
    }

    private static object FromInt64(long value, TypeCode target)
    {
        switch(target) {
            case TypeCode.SByte:
                return checked((sbyte) value);
            case TypeCode.Byte:
                return checked((byte) value);
            case TypeCode.Int16:
                return checked((short) value);
            case TypeCode.UInt16:
                return checked((ushort) value);
            case TypeCode.Int32:
                return checked((int) value);
            case TypeCode.UInt32:
                return checked((uint) value);
            case TypeCode.Int64:
                return value;
            case TypeCode.UInt64:
                return checked((ulong) value);
            case TypeCode.Single:
                return (float) value;
            case TypeCode.Double:
                return (double) value;
            case TypeCode.Decimal:
                return (decimal) value;
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }

    private static object FromUInt64(ulong value, TypeCode target)
    {
        switch(target) {
            case TypeCode.SByte:
                return checked((sbyte) value);
            case TypeCode.Byte:
                return checked((byte) value);
            case TypeCode.Int16:
                return checked((short) value);
            case TypeCode.UInt16:
                return checked((ushort) value);
            case TypeCode.Int32:
                return checked((int) value);
            case TypeCode.UInt32:
                return checked((uint) value);
            case TypeCode.Int64:
                return checked((long) value);
            case TypeCode.UInt64:
                return value;
            case TypeCode.Single:
                return (float) value;
            case TypeCode.Double:
                return (double) value;
            case TypeCode.Decimal:
                return (decimal) value;
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }

    private static object FromDouble(double value, TypeCode target)
    {
        // A checked conversion from double to an integral type truncates toward zero, and throws
        // OverflowException for NaN, infinity and values outside the target range.
        switch(target) {
            case TypeCode.SByte:
                return checked((sbyte) value);
            case TypeCode.Byte:
                return checked((byte) value);
            case TypeCode.Int16:
                return checked((short) value);
            case TypeCode.UInt16:
                return checked((ushort) value);
            case TypeCode.Int32:
                return checked((int) value);
            case TypeCode.UInt32:
                return checked((uint) value);
            case TypeCode.Int64:
                return checked((long) value);
            case TypeCode.UInt64:
                return checked((ulong) value);
            case TypeCode.Single:
                // the cast itself never throws: it turns a finite value beyond the float range into infinity
                var single = (float) value;
                if(float.IsInfinity(single) && double.IsFinite(value)) {
                    throw new OverflowException();
                }
                return single;
            case TypeCode.Double:
                return value;
            case TypeCode.Decimal:
                // throws OverflowException for NaN, infinity and values outside the decimal range
                return (decimal) value;
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }
}