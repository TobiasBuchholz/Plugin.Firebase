using Plugin.Firebase.Auth.Internal;

namespace Plugin.Firebase.UnitTests.Auth;

public class ClaimValueConverterTests
{
    public enum ByteLevel : byte
    {
        None = 0,
        Max = 255
    }

    public enum LongLevel : long
    {
        Min = long.MinValue,
        Max = long.MaxValue
    }

    public static TheoryData<object, Type, object> InRangeValues => new() {
        // every integral boundary, read from the 64-bit values the platforms widen claims to
        { (long) sbyte.MinValue, typeof(sbyte), sbyte.MinValue },
        { (long) sbyte.MaxValue, typeof(sbyte), sbyte.MaxValue },
        { 0L, typeof(byte), byte.MinValue },
        { (long) byte.MaxValue, typeof(byte), byte.MaxValue },
        { (long) short.MinValue, typeof(short), short.MinValue },
        { (long) short.MaxValue, typeof(short), short.MaxValue },
        { 0L, typeof(ushort), ushort.MinValue },
        { (long) ushort.MaxValue, typeof(ushort), ushort.MaxValue },
        { (long) int.MinValue, typeof(int), int.MinValue },
        { (long) int.MaxValue, typeof(int), int.MaxValue },
        { 0L, typeof(uint), uint.MinValue },
        { (long) uint.MaxValue, typeof(uint), uint.MaxValue },
        { long.MinValue, typeof(long), long.MinValue },
        { long.MaxValue, typeof(long), long.MaxValue },
        { 0L, typeof(ulong), ulong.MinValue },
        { ulong.MaxValue, typeof(ulong), ulong.MaxValue },
        { (ulong) long.MaxValue, typeof(long), long.MaxValue },

        // the natural types Android reads claims into
        { 7, typeof(long), 7L },
        { 7, typeof(byte), (byte) 7 },
        { 7L, typeof(int), 7 },
        { 7f, typeof(int), 7 },
        { 1.5f, typeof(double), 1.5 },

        // one claim, read as each numeric type
        { 300L, typeof(short), (short) 300 },
        { 300L, typeof(ushort), (ushort) 300 },
        { 300L, typeof(int), 300 },
        { 300L, typeof(uint), 300U },
        { 300L, typeof(long), 300L },
        { 300L, typeof(ulong), 300UL },
        { 300L, typeof(float), 300f },
        { 300L, typeof(double), 300.0 },
        { 300L, typeof(decimal), 300m },

        // fractional values truncate toward zero
        { 2.7, typeof(int), 2 },
        { -2.7, typeof(int), -2 },
        { 2.7, typeof(long), 2L },
        { -2.7, typeof(long), -2L },
        { 2.5f, typeof(int), 2 },
        { 254.9, typeof(byte), (byte) 254 },
        { -127.9, typeof(sbyte), (sbyte) -127 },
        { 2147483646.9, typeof(int), 2147483646 },
        { -2147483647.9, typeof(int), -2147483647 },
        { 0.5, typeof(uint), 0U },
        { 2.0, typeof(ulong), 2UL },

        // integral boundaries read from doubles
        { (double) sbyte.MinValue, typeof(sbyte), sbyte.MinValue },
        { (double) sbyte.MaxValue, typeof(sbyte), sbyte.MaxValue },
        { (double) byte.MinValue, typeof(byte), byte.MinValue },
        { (double) byte.MaxValue, typeof(byte), byte.MaxValue },
        { (double) short.MinValue, typeof(short), short.MinValue },
        { (double) short.MaxValue, typeof(short), short.MaxValue },
        { (double) ushort.MinValue, typeof(ushort), ushort.MinValue },
        { (double) ushort.MaxValue, typeof(ushort), ushort.MaxValue },
        { (double) int.MinValue, typeof(int), int.MinValue },
        { (double) int.MaxValue, typeof(int), int.MaxValue },
        { (double) uint.MinValue, typeof(uint), uint.MinValue },
        { (double) uint.MaxValue, typeof(uint), uint.MaxValue },
        { -9223372036854775808.0, typeof(long), long.MinValue },
        // 2^63 - 1024: the largest double below 2^63
        { 9223372036854774784.0, typeof(long), 9223372036854774784L },
        { 0.0, typeof(ulong), ulong.MinValue },
        // 2^64 - 2048: the largest double below 2^64
        { 18446744073709549568.0, typeof(ulong), 18446744073709549568UL },

        // floating-point and decimal targets keep the fraction
        { 1.5, typeof(float), 1.5f },
        { 1.5, typeof(double), 1.5 },
        { 1.5, typeof(decimal), 1.5m },
        { -2.7, typeof(decimal), -2.7m },
        { ulong.MaxValue, typeof(decimal), 18446744073709551615m },
        { (double) float.MaxValue, typeof(float), float.MaxValue },
        { double.NaN, typeof(double), double.NaN },
        { double.PositiveInfinity, typeof(double), double.PositiveInfinity },
        { double.NegativeInfinity, typeof(float), float.NegativeInfinity },

        // enums convert through their underlying type, and fractions truncate
        { 0L, typeof(ByteLevel), ByteLevel.None },
        { 255L, typeof(ByteLevel), ByteLevel.Max },
        { 2.7, typeof(ByteLevel), (ByteLevel) 2 },
        { 7L, typeof(ByteLevel), (ByteLevel) 7 },
        { long.MinValue, typeof(LongLevel), LongLevel.Min },
        { (ulong) long.MaxValue, typeof(LongLevel), LongLevel.Max },

        // nullable targets convert to their underlying type
        { 7L, typeof(int?), 7 },
        { -2.7, typeof(int?), -2 },
        { 1.5, typeof(decimal?), 1.5m },
        { 255L, typeof(ByteLevel?), ByteLevel.Max },
    };

    public static TheoryData<object, Type> OutOfRangeValues => new() {
        // one past every integral boundary
        { (long) sbyte.MinValue - 1, typeof(sbyte) },
        { (long) sbyte.MaxValue + 1, typeof(sbyte) },
        { -1L, typeof(byte) },
        { (long) byte.MaxValue + 1, typeof(byte) },
        { (long) short.MinValue - 1, typeof(short) },
        { (long) short.MaxValue + 1, typeof(short) },
        { -1L, typeof(ushort) },
        { (long) ushort.MaxValue + 1, typeof(ushort) },
        { (long) int.MinValue - 1, typeof(int) },
        { (long) int.MaxValue + 1, typeof(int) },
        { -1L, typeof(uint) },
        { (long) uint.MaxValue + 1, typeof(uint) },
        { (ulong) long.MaxValue + 1, typeof(long) },
        { -1L, typeof(ulong) },
        { 256UL, typeof(byte) },
        { 300, typeof(byte) },

        // doubles are range checked before truncation, so a fraction past a boundary doesn't fit
        { 127.9, typeof(sbyte) },
        { -128.5, typeof(sbyte) },
        { 255.9, typeof(byte) },
        { -0.5, typeof(byte) },
        { 32767.9, typeof(short) },
        { -32768.5, typeof(short) },
        { 65535.9, typeof(ushort) },
        { -0.5, typeof(ushort) },
        { 2147483647.9, typeof(int) },
        { -2147483648.5, typeof(int) },
        { 4294967295.9, typeof(uint) },
        { -0.5, typeof(uint) },
        { -0.5, typeof(ulong) },

        // whole doubles past a boundary
        { 256.0, typeof(byte) },
        { -1.0, typeof(byte) },
        { -129.0, typeof(sbyte) },
        { 2147483648.0, typeof(int) },
        { -2147483649.0, typeof(int) },
        // 2^63, which is what (double) long.MaxValue rounds to
        { 9223372036854775808.0, typeof(long) },
        { 9.3e18, typeof(long) },
        { -9.3e18, typeof(long) },
        { -1.0, typeof(ulong) },
        // 2^64, which is what (double) ulong.MaxValue rounds to
        { 18446744073709551616.0, typeof(ulong) },
        { 1.9e19, typeof(ulong) },

        // NaN and infinity can't become an integral, enum or decimal value
        { double.NaN, typeof(int) },
        { double.NaN, typeof(long) },
        { double.NaN, typeof(ulong) },
        { double.PositiveInfinity, typeof(long) },
        { double.NegativeInfinity, typeof(int) },
        { float.NaN, typeof(byte) },
        { double.NaN, typeof(decimal) },
        { double.PositiveInfinity, typeof(decimal) },
        { double.NaN, typeof(ByteLevel) },

        // float and decimal ranges
        { 1e39, typeof(float) },
        { -1e39, typeof(float) },
        { double.MaxValue, typeof(float) },
        { 1e29, typeof(decimal) },

        // enums are range checked against their underlying type
        { 256L, typeof(ByteLevel) },
        { 300L, typeof(ByteLevel) },
        { -1L, typeof(ByteLevel) },
        { 255.9, typeof(ByteLevel) },
        { 256.5, typeof(ByteLevel) },
        { ulong.MaxValue, typeof(LongLevel) },

        // nullable targets keep the range check
        { 300L, typeof(byte?) },
        { 256L, typeof(ByteLevel?) },
        { double.NaN, typeof(int?) },
    };

    public static TheoryData<object, Type> MismatchedValues => new() {
        // strings are never parsed into numbers
        { "123", typeof(long) },
        { "123", typeof(int?) },
        { "2.5", typeof(double) },
        { "2.5", typeof(decimal) },
        { "255", typeof(ByteLevel) },

        // booleans are never read as 1 or 0
        { true, typeof(int) },
        { false, typeof(byte) },
        { true, typeof(long?) },
        { true, typeof(double) },
        { true, typeof(decimal) },
        { true, typeof(ByteLevel) },

        // char targets are rejected whatever the claim holds
        { 65L, typeof(char) },
        { 65.0, typeof(char?) },
        { "A", typeof(char) },

        // anything else that isn't a number
        { DateTimeOffset.UnixEpoch, typeof(long) },
    };

    public static TheoryData<object, Type?> PassThroughValues => new() {
        { "text", typeof(string) },
        { "123", typeof(string) },
        { "123", null },
        { true, typeof(bool) },
        { false, typeof(bool?) },
        { true, null },
        { 7L, typeof(object) },
        { 7, null },
        { 1.5, null },
        // mismatched non-numeric targets are left for the caller's cast to reject
        { 7L, typeof(string) },
        { 1.5, typeof(bool) },
        { true, typeof(string) },
    };

    [Theory]
    [MemberData(nameof(InRangeValues))]
    public void converts_value_to_requested_numeric_type(object value, Type targetType, object expected)
    {
        var result = ClaimValueConverter.ToRequestedType(value, targetType);

        Assert.IsType(expected.GetType(), result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(OutOfRangeValues))]
    public void throws_overflow_for_value_outside_requested_type(object value, Type targetType)
    {
        Assert.Throws<OverflowException>(() => ClaimValueConverter.ToRequestedType(value, targetType));
    }

    [Theory]
    [MemberData(nameof(MismatchedValues))]
    public void throws_invalid_cast_for_value_of_another_type(object value, Type targetType)
    {
        Assert.Throws<InvalidCastException>(() => ClaimValueConverter.ToRequestedType(value, targetType));
    }

    [Theory]
    [MemberData(nameof(PassThroughValues))]
    public void returns_value_unchanged_for_non_numeric_target(object value, Type? targetType)
    {
        Assert.Same(value, ClaimValueConverter.ToRequestedType(value, targetType));
    }

    [Fact]
    public void overflow_message_names_value_and_requested_type()
    {
        var exception = Assert.Throws<OverflowException>(
            () => ClaimValueConverter.ToRequestedType(300L, typeof(byte))
        );

        Assert.Contains("300", exception.Message);
        Assert.Contains(typeof(byte).FullName!, exception.Message);
    }

    [Fact]
    public void invalid_cast_message_names_claim_type_and_requested_type()
    {
        var exception = Assert.Throws<InvalidCastException>(
            () => ClaimValueConverter.ToRequestedType("123", typeof(long))
        );

        Assert.Contains(typeof(string).FullName!, exception.Message);
        Assert.Contains(typeof(long).FullName!, exception.Message);
    }
}