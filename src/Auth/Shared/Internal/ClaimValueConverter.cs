using System.Globalization;
using System.Numerics;

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
    /// A number in plain decimal notation, such as an NSDecimalNumber's <c>StringValue</c>, or <c>NaN</c>. It is
    /// converted from its text, so the range check sees its exact value and no digits are lost by first rounding it to
    /// a <see cref="double"/> or <see cref="decimal"/>.
    /// </summary>
    internal readonly record struct DecimalText(string Text)
    {
        public override string ToString() => Text;
    }

    /// <summary>
    /// Converts <paramref name="value"/> to <paramref name="targetType"/>. A numeric, enum or <see cref="char"/>
    /// target, or a nullable one, gets a converted number. Any other target gets <paramref name="value"/> itself when
    /// the value is already of that type.
    /// </summary>
    /// <param name="value">
    /// The claim value: a string, a bool, a CLR integral, floating-point or <see cref="decimal"/> number, or a
    /// <see cref="DecimalText"/>.
    /// </param>
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

    /// <summary>
    /// Returns a list or dictionary built for <paramref name="targetType"/>, or throws
    /// <see cref="InvalidCastException"/> when the claim is an array or object but the target isn't a list or
    /// dictionary type, for example a nested object in a dictionary read with <see cref="string"/> values.
    /// </summary>
    public static object ToRequestedContainer(object container, Type? targetType)
    {
        return targetType is null || targetType.IsInstanceOfType(container)
            ? container
            : throw Mismatch(container, targetType);
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
                case decimal number:
                    return FromDecimal(number, typeCode);
                case DecimalText number:
                    return FromDecimalText(number.Text, typeCode);
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
                // NaN and infinity throw here, values outside the decimal range in Parse. The cast (decimal) value
                // would keep only 15 significant digits, so 2^53 + 2 would come back as 9007199254740990.
                if(!double.IsFinite(value)) {
                    throw new OverflowException();
                }
                return decimal.Parse(
                    value.ToString("R", CultureInfo.InvariantCulture),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture
                );
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }

    private static object FromDecimal(decimal value, TypeCode target)
    {
        // Every integral bound is exact as a decimal, so the range check sees the value as it is and the cast that
        // follows only truncates toward zero.
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
                return (long) InRange(value, long.MinValue, long.MaxValue);
            case TypeCode.UInt64:
                return (ulong) InRange(value, ulong.MinValue, ulong.MaxValue);
            case TypeCode.Single:
                // the decimal range is far inside the float range, so this only rounds
                return (float) value;
            case TypeCode.Double:
                return (double) value;
            case TypeCode.Decimal:
                return value;
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }

    private static object FromDecimalText(string text, TypeCode target)
    {
        var approximation = double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
        if(double.IsNaN(approximation)) {
            return FromDouble(approximation, target);
        }

        switch(target) {
            case TypeCode.Single:
                // parsed from the text, so the result is correctly rounded; a finite value past float's range throws
                var single = float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
                return float.IsInfinity(single) ? throw new OverflowException() : single;
            case TypeCode.Double:
                return approximation;
            case TypeCode.Decimal:
                // rounds to the 28 or 29 significant digits a decimal holds, like any decimal parse
                return decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                    ? value
                    : throw new OverflowException();
            default:
                return FromIntegralText(text, target);
        }
    }

    private static object FromIntegralText(string text, TypeCode target)
    {
        // Split the exact value into its integer part and whether it has a fraction, without rounding either.
        var point = text.IndexOf('.');
        var integerText = point < 0 ? text : text[..point];
        var hasFraction = point >= 0 && text.AsSpan(point + 1).IndexOfAnyExcept('0') >= 0;
        var integer = integerText is "" or "-" or "+"
            ? BigInteger.Zero
            : BigInteger.Parse(integerText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

        // Range check before truncating: an integer part at a bound only fits without a fraction past that bound, so
        // max + 0.5 and min - 0.5 (including -0.5 for an unsigned type) throw.
        var (min, max) = IntegralRange(target);
        var isNegative = text.StartsWith('-');
        var fits = integer >= min && integer <= max && !(hasFraction && integer == (isNegative ? min : max));
        if(!fits) {
            throw new OverflowException();
        }

        switch(target) {
            case TypeCode.SByte:
                return (sbyte) integer;
            case TypeCode.Byte:
                return (byte) integer;
            case TypeCode.Int16:
                return (short) integer;
            case TypeCode.UInt16:
                return (ushort) integer;
            case TypeCode.Int32:
                return (int) integer;
            case TypeCode.UInt32:
                return (uint) integer;
            case TypeCode.Int64:
                return (long) integer;
            case TypeCode.UInt64:
                return (ulong) integer;
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }

    private static (BigInteger Min, BigInteger Max) IntegralRange(TypeCode target)
    {
        switch(target) {
            case TypeCode.SByte:
                return (sbyte.MinValue, sbyte.MaxValue);
            case TypeCode.Byte:
                return (byte.MinValue, byte.MaxValue);
            case TypeCode.Int16:
                return (short.MinValue, short.MaxValue);
            case TypeCode.UInt16:
                return (ushort.MinValue, ushort.MaxValue);
            case TypeCode.Int32:
                return (int.MinValue, int.MaxValue);
            case TypeCode.UInt32:
                return (uint.MinValue, uint.MaxValue);
            case TypeCode.Int64:
                return (long.MinValue, long.MaxValue);
            case TypeCode.UInt64:
                return (ulong.MinValue, ulong.MaxValue);
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, null);
        }
    }

    private static decimal InRange(decimal value, decimal min, decimal max)
    {
        // an integral value at max + 0.9 doesn't fit, even though truncating it would give max
        return value >= min && value <= max ? value : throw new OverflowException();
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
        var valueType = value is DecimalText ? typeof(decimal) : value.GetType();
        return new InvalidCastException($"Can't read a claim of type {valueType} as {targetType}.");
    }
}