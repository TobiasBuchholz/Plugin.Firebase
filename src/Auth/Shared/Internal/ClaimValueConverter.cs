using System.Globalization;

namespace Plugin.Firebase.Auth.Internal;

/// <summary>
/// Converts a scalar claim value, already read from the native token into its natural .NET type, into the type
/// requested through <see cref="IAuthTokenResult.GetClaim{T}(string)"/>, or into the element type of a requested list
/// or dictionary.
/// </summary>
/// <remarks>
/// A value that doesn't fit the target throws <see cref="OverflowException"/>; NaN and infinity only fit
/// <see cref="float"/> and <see cref="double"/>. A fractional value read into an integral or enum type is range checked
/// as it is and then truncated toward zero, so 2.7 becomes 2 but 2147483647.9 doesn't fit <see cref="int"/>. Values
/// are never coerced between JSON types: a mismatch throws <see cref="InvalidCastException"/>.
/// </remarks>
internal static class ClaimValueConverter
{
    /// <summary>
    /// Converts <paramref name="value"/> to <paramref name="targetType"/>. A numeric, enum or <see cref="char"/>
    /// target, or a nullable one, gets a converted number. Any other target gets <paramref name="value"/> itself when
    /// the value is already of that type.
    /// </summary>
    /// <param name="value">The claim value: a string, a bool, or a CLR integral or floating-point number.</param>
    /// <param name="targetType">The requested type, or null to keep the natural type.</param>
    /// <returns>The converted value, or <paramref name="value"/> itself for a non-numeric target.</returns>
    /// <exception cref="OverflowException">
    /// The value doesn't fit the numeric or enum target, or it is NaN or infinite and the target is integral, an enum
    /// or <see cref="decimal"/>.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// The value isn't a number but the target is numeric or an enum, the target is <see cref="char"/>, or the target
    /// is any other type the value isn't an instance of.
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

        if(IsNumeric(conversionType)) {
            return ToNumber(value, conversionType, targetType);
        }

        // checked here rather than left to the caller's cast, so list elements and dictionary values fail the same way
        return conversionType.IsInstanceOfType(value) ? value : throw Mismatch(value, targetType);
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
                case sbyte or short or int or long or byte or ushort or uint or ulong:
                    return FromInteger(value, typeCode);
                case float or double:
                    return FromDouble(Convert.ToDouble(value, CultureInfo.InvariantCulture), typeCode);
                default:
                    throw Mismatch(value, targetType);
            }
        } catch(OverflowException e) {
            throw new OverflowException(
                FormattableString.Invariant($"The claim value {value} doesn't fit into {targetType}."),
                e
            );
        }
    }

    private static object FromInteger(object value, TypeCode target)
    {
        // Only integers get here, so Convert never parses or rounds: it either converts exactly or throws
        // OverflowException because the value is out of range.
        switch(target) {
            case TypeCode.SByte:
                return Convert.ToSByte(value, CultureInfo.InvariantCulture);
            case TypeCode.Byte:
                return Convert.ToByte(value, CultureInfo.InvariantCulture);
            case TypeCode.Int16:
                return Convert.ToInt16(value, CultureInfo.InvariantCulture);
            case TypeCode.UInt16:
                return Convert.ToUInt16(value, CultureInfo.InvariantCulture);
            case TypeCode.Int32:
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            case TypeCode.UInt32:
                return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
            case TypeCode.Int64:
                return Convert.ToInt64(value, CultureInfo.InvariantCulture);
            case TypeCode.UInt64:
                return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            case TypeCode.Single:
                return Convert.ToSingle(value, CultureInfo.InvariantCulture);
            case TypeCode.Double:
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            case TypeCode.Decimal:
                return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }

    private static object FromDouble(double value, TypeCode target)
    {
        // Integral targets range check the raw value and only then truncate toward zero, as native Android's
        // CustomClassMapper does. A checked cast alone would truncate first and accept 2147483647.9 as int.MaxValue.
        switch(target) {
            case TypeCode.SByte:
                return (sbyte) InRange(value, sbyte.MinValue, sbyte.MaxValue);
            case TypeCode.Byte:
                return (byte) InRange(value, byte.MinValue, byte.MaxValue);
            case TypeCode.Int16:
                return (short) InRange(value, short.MinValue, short.MaxValue);
            case TypeCode.UInt16:
                return (ushort) InRange(value, ushort.MinValue, ushort.MaxValue);
            case TypeCode.Int32:
                return (int) InRange(value, int.MinValue, int.MaxValue);
            case TypeCode.UInt32:
                return (uint) InRange(value, uint.MinValue, uint.MaxValue);
            case TypeCode.Int64:
                // (double) long.MaxValue rounds up to 2^63, which doesn't fit, so the upper bound is exclusive
                return (long) InRange(value, -9223372036854775808.0, 9223372036854775808.0, maxIsExclusive: true);
            case TypeCode.UInt64:
                // (double) ulong.MaxValue rounds up to 2^64, which doesn't fit, so the upper bound is exclusive
                return (ulong) InRange(value, 0.0, 18446744073709551616.0, maxIsExclusive: true);
            case TypeCode.Single:
                // the cast never throws: a finite value that would round to infinity has to be caught here
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

    private static double InRange(double value, double min, double max, bool maxIsExclusive = false)
    {
        // NaN fails every comparison, so it throws along with infinity and out-of-range values. Once the value is in
        // range, the plain cast that follows truncates toward zero and can't overflow.
        var fits = value >= min && (maxIsExclusive ? value < max : value <= max);
        if(!fits) {
            throw new OverflowException();
        }
        return value;
    }

    private static InvalidCastException Mismatch(object value, Type targetType)
    {
        return new InvalidCastException($"Can't read a claim of type {value.GetType()} as {targetType}.");
    }
}