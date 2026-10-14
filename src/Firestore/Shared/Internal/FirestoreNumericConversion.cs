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
/// <item>A fractional value read into an integral or enum target is truncated toward zero.</item>
/// <item>
/// A value outside the target's range throws <see cref="OverflowException"/>. So does NaN or
/// infinity read into an integral, enum or <see cref="decimal"/> target, and a finite value too
/// large for a <see cref="float"/> target.
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
            return Type.GetTypeCode(conversionType) switch {
                TypeCode.Double => Convert.ToDouble(value, CultureInfo.InvariantCulture),
                TypeCode.Single => ToSingle(value),
                TypeCode.Decimal => Convert.ToDecimal(RequireFinite(value), CultureInfo.InvariantCulture),
                // Integral targets. Truncating first leaves Convert only the range check; on its own it
                // would round to even.
                _ => Convert.ChangeType(Truncate(RequireFinite(value)), conversionType, CultureInfo.InvariantCulture)!
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

    private static object Truncate(object value)
    {
        return value switch {
            double d => Math.Truncate(d),
            float f => MathF.Truncate(f),
            decimal m => decimal.Truncate(m),
            _ => value
        };
    }
}