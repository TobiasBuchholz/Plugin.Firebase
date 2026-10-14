using System.Numerics;
using Plugin.Firebase.Firestore.Internal;

namespace Plugin.Firebase.UnitTests;

public class FirestoreNumericWritesTests
{
    public static TheoryData<object, long> IntegralValues => new() {
        { sbyte.MinValue, -128L },
        { sbyte.MaxValue, 127L },
        { byte.MinValue, 0L },
        { byte.MaxValue, 255L },
        { short.MinValue, -32768L },
        { short.MaxValue, 32767L },
        { ushort.MinValue, 0L },
        { ushort.MaxValue, 65535L },
        { int.MinValue, -2147483648L },
        { int.MaxValue, 2147483647L },
        { uint.MinValue, 0L },
        { uint.MaxValue, 4294967295L },
        { long.MinValue, long.MinValue },
        { long.MaxValue, long.MaxValue },
        { ulong.MinValue, 0L },
        { (ulong) long.MaxValue, long.MaxValue }
    };

    public static TheoryData<object, long> EnumValues => new() {
        { SByteEnum.Min, -128L },
        { SByteEnum.Max, 127L },
        { ByteEnum.Min, 0L },
        { ByteEnum.Max, 255L },
        { ShortEnum.Min, -32768L },
        { ShortEnum.Max, 32767L },
        { UShortEnum.Min, 0L },
        { UShortEnum.Max, 65535L },
        { IntEnum.Min, -2147483648L },
        { IntEnum.Max, 2147483647L },
        { UIntEnum.Min, 0L },
        { UIntEnum.Max, 4294967295L },
        { LongEnum.Min, long.MinValue },
        { LongEnum.Max, long.MaxValue },
        { ULongEnum.Min, 0L },
        { ULongEnum.Int64Max, long.MaxValue },
        // values without a named member are written the same way
        { (UIntEnum) 3_000_000_000, 3_000_000_000L }
    };

    public static TheoryData<object, string> UnsupportedValues => new() {
        { 19.99m, "decimal" },
        { 'A', "character" },
        { (nint) 1, "64-bit integers or doubles" },
        { (nuint) 1, "64-bit integers or doubles" },
        { (Half) 1.5, "64-bit integers or doubles" },
        { (Int128) 1, "64-bit integers or doubles" },
        { (UInt128) 1, "64-bit integers or doubles" },
        { new BigInteger(1), "64-bit integers or doubles" }
    };

    public static IEnumerable<object?[]> NonNumericValues => [
        [null],
        ["5"],
        [true],
        [new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)],
        [new object()]
    ];

    [Theory]
    [MemberData(nameof(IntegralValues))]
    public void widens_integral_values_to_int64(object value, long expected)
    {
        Assert.True(FirestoreNumericWrites.TryWiden(value, out var widened));
        Assert.IsType<long>(widened);
        Assert.Equal(expected, (long) widened);
    }

    [Theory]
    [MemberData(nameof(EnumValues))]
    public void widens_enums_through_their_underlying_type_without_wrapping(object value, long expected)
    {
        Assert.True(FirestoreNumericWrites.TryWiden(value, out var widened));
        Assert.IsType<long>(widened);
        Assert.Equal(expected, (long) widened);
    }

    [Theory]
    [InlineData(2.5f, 2.5)]
    [InlineData(0.1f, (double) 0.1f)]
    [InlineData(float.MinValue, (double) float.MinValue)]
    [InlineData(float.MaxValue, (double) float.MaxValue)]
    [InlineData(float.PositiveInfinity, double.PositiveInfinity)]
    public void widens_float_values_to_double(float value, double expected)
    {
        Assert.True(FirestoreNumericWrites.TryWiden(value, out var widened));
        Assert.IsType<double>(widened);
        Assert.Equal(expected, (double) widened);
    }

    [Theory]
    [InlineData(2.25)]
    [InlineData(2.0)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    [InlineData(double.NaN)]
    public void keeps_double_values_as_double(double value)
    {
        Assert.True(FirestoreNumericWrites.TryWiden(value, out var widened));
        Assert.IsType<double>(widened);
        Assert.Equal(value, (double) widened);
    }

    [Theory]
    [MemberData(nameof(UnsupportedValues))]
    public void rejects_numeric_types_firestore_cannot_represent(object value, string reason)
    {
        var exception = Assert.Throws<ArgumentException>(() => FirestoreNumericWrites.TryWiden(value, out _));

        Assert.Contains(reason, exception.Message);
        Assert.Contains(value.GetType().ToString(), exception.Message);
    }

    [Fact]
    public void rejects_ulong_values_above_int64_max()
    {
        var justAbove = Assert.Throws<ArgumentException>(() => FirestoreNumericWrites.TryWiden((ulong) long.MaxValue + 1, out _));
        var max = Assert.Throws<ArgumentException>(() => FirestoreNumericWrites.TryWiden(ulong.MaxValue, out _));

        Assert.Contains("System.UInt64 value 9223372036854775808", justAbove.Message);
        Assert.Contains("System.UInt64 value 18446744073709551615", max.Message);
    }

    [Fact]
    public void rejects_ulong_enum_values_above_int64_max()
    {
        var exception = Assert.Throws<ArgumentException>(() => FirestoreNumericWrites.TryWiden(ULongEnum.AboveInt64Max, out _));

        Assert.Contains($"{typeof(ULongEnum)} value 9223372036854775808", exception.Message);
    }

    [Theory]
    [MemberData(nameof(NonNumericValues))]
    public void leaves_non_numeric_values_to_the_platform_conversion(object? value)
    {
        Assert.False(FirestoreNumericWrites.TryWiden(value, out var widened));
        Assert.Null(widened);
    }

    public enum SByteEnum : sbyte { Min = sbyte.MinValue, Max = sbyte.MaxValue }

    public enum ByteEnum : byte { Min = byte.MinValue, Max = byte.MaxValue }

    public enum ShortEnum : short { Min = short.MinValue, Max = short.MaxValue }

    public enum UShortEnum : ushort { Min = ushort.MinValue, Max = ushort.MaxValue }

    public enum IntEnum { Min = int.MinValue, Max = int.MaxValue }

    public enum UIntEnum : uint { Min = uint.MinValue, Max = uint.MaxValue }

    public enum LongEnum : long { Min = long.MinValue, Max = long.MaxValue }

    public enum ULongEnum : ulong { Min = ulong.MinValue, Int64Max = long.MaxValue, AboveInt64Max = (ulong) long.MaxValue + 1 }
}