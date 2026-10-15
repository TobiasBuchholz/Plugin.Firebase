using Plugin.Firebase.Firestore.Internal;

namespace Plugin.Firebase.UnitTests.Firestore;

public class FirestoreNumericConversionTests
{
    public enum SByteLevel : sbyte { Min = sbyte.MinValue, Max = sbyte.MaxValue }
    public enum ByteLevel : byte { None, Low, High }
    public enum ShortLevel : short { Min = short.MinValue, Max = short.MaxValue }
    public enum UShortLevel : ushort { Max = ushort.MaxValue }
    public enum IntLevel { Min = int.MinValue, Max = int.MaxValue }
    public enum UIntLevel : uint { Max = uint.MaxValue }
    public enum LongLevel : long { Min = long.MinValue, Max = long.MaxValue }
    public enum ULongLevel : ulong { Max = ulong.MaxValue }

    [Flags]
    public enum Access : byte { None = 0, Read = 1, Write = 2, Delete = 4 }

    // Admin and All only fit a uint: a cast to int turns them into negative numbers.
    [Flags]
    public enum Permissions : uint { None = 0, Read = 1, Write = 2, Admin = 0x80000000, All = 0xFFFFFFFF }

    // Firestore returns whole numbers as long and fractional numbers as double; the other sources
    // pin that every .NET numeric primitive follows the same rules.
    public static TheoryData<object, Type, object> Conversions => new() {
        // Integral targets at their exact boundaries.
        { -128L, typeof(sbyte), (sbyte) -128 },
        { 127L, typeof(sbyte), (sbyte) 127 },
        { 0L, typeof(byte), (byte) 0 },
        { 255L, typeof(byte), (byte) 255 },
        { -32768L, typeof(short), (short) -32768 },
        { 32767L, typeof(short), (short) 32767 },
        { 0L, typeof(ushort), (ushort) 0 },
        { 65535L, typeof(ushort), (ushort) 65535 },
        { (long) int.MinValue, typeof(int), int.MinValue },
        { (long) int.MaxValue, typeof(int), int.MaxValue },
        { 0L, typeof(uint), 0U },
        { (long) uint.MaxValue, typeof(uint), uint.MaxValue },
        { long.MinValue, typeof(long), long.MinValue },
        { long.MaxValue, typeof(long), long.MaxValue },
        { 0L, typeof(ulong), 0UL },
        { long.MaxValue, typeof(ulong), (ulong) long.MaxValue },
        { ulong.MaxValue, typeof(ulong), ulong.MaxValue },

        // Doubles at the exact boundaries convert. 2^63 - 1024 and 2^64 - 2048 are the largest
        // doubles below 2^63 and 2^64.
        { -128.0, typeof(sbyte), (sbyte) -128 },
        { 127.0, typeof(sbyte), (sbyte) 127 },
        { 0.0, typeof(byte), (byte) 0 },
        { 255.0, typeof(byte), (byte) 255 },
        { -32768.0, typeof(short), (short) -32768 },
        { 32767.0, typeof(short), (short) 32767 },
        { 65535.0, typeof(ushort), (ushort) 65535 },
        { -2147483648.0, typeof(int), int.MinValue },
        { 2147483647.0, typeof(int), int.MaxValue },
        { 4294967295.0, typeof(uint), uint.MaxValue },
        { -9223372036854775808.0, typeof(long), long.MinValue },
        { -9223372036854775808f, typeof(long), long.MinValue },
        { 9223372036854774784.0, typeof(long), 9223372036854774784L },
        { 18446744073709549568.0, typeof(ulong), 18446744073709549568UL },

        // Fractional values inside the range truncate toward zero.
        { 2.7, typeof(int), 2 },
        { -2.7, typeof(int), -2 },
        { 2.5, typeof(int), 2 },
        { 3.5, typeof(int), 3 },
        { -2.5, typeof(long), -2L },
        { 2.7, typeof(long), 2L },
        { 3.0, typeof(long), 3L },
        { -0.0, typeof(int), 0 },
        { -0.0, typeof(ulong), 0UL },
        { 0.9, typeof(byte), (byte) 0 },
        { 254.9, typeof(byte), (byte) 254 },
        { -127.9, typeof(sbyte), (sbyte) -127 },
        { 2147483646.9, typeof(int), 2147483646 },
        { -2147483647.9, typeof(int), -2147483647 },
        { 2.7f, typeof(short), (short) 2 },
        { -2.7m, typeof(long), -2L },
        { 254.9m, typeof(byte), (byte) 254 },

        // Floating point targets keep the value, including NaN and infinity.
        { 2L, typeof(double), 2.0 },
        { 2.25, typeof(float), 2.25f },
        { (double) float.MaxValue, typeof(float), float.MaxValue },
        { double.NaN, typeof(float), float.NaN },
        { double.PositiveInfinity, typeof(float), float.PositiveInfinity },
        { long.MaxValue, typeof(float), (float) long.MaxValue },

        // Decimal targets.
        { 12345L, typeof(decimal), 12345m },
        { 12.5, typeof(decimal), 12.5m },
        { -2.7, typeof(decimal), -2.7m },
        { ulong.MaxValue, typeof(decimal), 18446744073709551615m },

        // Enums convert through their underlying type, keeping in-range values that aren't members.
        { -128L, typeof(SByteLevel), SByteLevel.Min },
        { 127L, typeof(SByteLevel), SByteLevel.Max },
        { 2L, typeof(ByteLevel), ByteLevel.High },
        { 255L, typeof(ByteLevel), (ByteLevel) 255 },
        { 2.7, typeof(ByteLevel), ByteLevel.High },
        { 0.5, typeof(ByteLevel), ByteLevel.None },
        { -32768L, typeof(ShortLevel), ShortLevel.Min },
        { 32767L, typeof(ShortLevel), ShortLevel.Max },
        { 65535L, typeof(UShortLevel), UShortLevel.Max },
        { (long) int.MinValue, typeof(IntLevel), IntLevel.Min },
        { (long) int.MaxValue, typeof(IntLevel), IntLevel.Max },
        { 7L, typeof(IntLevel), (IntLevel) 7 },
        { (long) uint.MaxValue, typeof(UIntLevel), UIntLevel.Max },
        { long.MinValue, typeof(LongLevel), LongLevel.Min },
        { long.MaxValue, typeof(LongLevel), LongLevel.Max },
        { ulong.MaxValue, typeof(ULongLevel), ULongLevel.Max },
        { 3L, typeof(Access), Access.Read | Access.Write },
        { 7.9, typeof(Access), Access.Read | Access.Write | Access.Delete },
        { 9L, typeof(Access), (Access) 9 },
        { 0L, typeof(UIntLevel), (UIntLevel) 0 },
        { 4294967295.0, typeof(UIntLevel), UIntLevel.Max },
        { -9223372036854775808.0, typeof(LongLevel), LongLevel.Min },
        { 0L, typeof(ULongLevel), (ULongLevel) 0 },
        // long.MaxValue is the largest integer Firestore stores, so it is the top edge for a ulong.
        { long.MaxValue, typeof(ULongLevel), (ULongLevel) long.MaxValue },
        { 3L, typeof(Permissions), Permissions.Read | Permissions.Write },
        { 2147483649L, typeof(Permissions), Permissions.Admin | Permissions.Read },
        { 4294967295L, typeof(Permissions), Permissions.All },

        // Nullable targets convert to their underlying type.
        { 2L, typeof(int?), 2 },
        { 2.7, typeof(long?), 2L },
        { 1.5, typeof(float?), 1.5f },
        { 12.5, typeof(decimal?), 12.5m },
        { 1L, typeof(ByteLevel?), ByteLevel.Low },
        { 255L, typeof(ByteLevel?), (ByteLevel) 255 },
        { 2.7, typeof(ByteLevel?), ByteLevel.High },
        { 4294967295L, typeof(Permissions?), Permissions.All }
    };

    public static TheoryData<object, Type> Overflows => new() {
        // Just outside each integral target.
        { -129L, typeof(sbyte) },
        { 128L, typeof(sbyte) },
        { -1L, typeof(byte) },
        { 256L, typeof(byte) },
        { -32769L, typeof(short) },
        { 32768L, typeof(short) },
        { -1L, typeof(ushort) },
        { 65536L, typeof(ushort) },
        { (long) int.MinValue - 1, typeof(int) },
        { (long) int.MaxValue + 1, typeof(int) },
        { -1L, typeof(uint) },
        { (long) uint.MaxValue + 1, typeof(uint) },
        { (ulong) long.MaxValue + 1, typeof(long) },
        { -1L, typeof(ulong) },
        { 300L, typeof(byte?) },

        // Fractional values are range checked as stored, before truncating: MaxValue + 0.9 and
        // MinValue - 0.5 don't fit, even though they would truncate into the range.
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
        { 255.5f, typeof(byte) },
        { 255.5m, typeof(byte) },
        { 9223372036854775807.5m, typeof(long) },
        { -2147483648.5, typeof(int?) },

        // Doubles just outside the range. (double) long.MaxValue rounds up to 2^63, which doesn't
        // fit a long, as a double or as a float; the next double below -2^63 is 2^63 + 2048 away
        // from zero.
        { 9223372036854775808.0, typeof(long) },
        { 9223372036854775808f, typeof(long) },
        { -9223372036854777856.0, typeof(long) },
        { 18446744073709551616.0, typeof(ulong) },
        { 256.0, typeof(byte) },
        { -1.0, typeof(byte) },
        { 2147483648.0, typeof(int) },

        // NaN and infinity have no integral or decimal value.
        { double.NaN, typeof(int) },
        { double.PositiveInfinity, typeof(long) },
        { double.NegativeInfinity, typeof(ulong) },
        { float.NaN, typeof(short) },
        { double.NaN, typeof(int?) },
        { double.NaN, typeof(decimal) },
        { double.PositiveInfinity, typeof(decimal) },
        { float.NegativeInfinity, typeof(decimal) },
        { float.NaN, typeof(decimal) },
        { double.NaN, typeof(ByteLevel) },

        // A finite double too large for the target.
        { 1e300, typeof(float) },
        { -1e300, typeof(float) },
        { 1e30, typeof(decimal) },

        // Enums are range checked against their underlying type.
        { -129L, typeof(SByteLevel) },
        { 128L, typeof(SByteLevel) },
        { 256L, typeof(ByteLevel) },
        { 300L, typeof(ByteLevel) },
        { -1.0, typeof(ByteLevel) },
        { -0.5, typeof(ByteLevel) },
        { 255.9, typeof(ByteLevel) },
        { 32768L, typeof(ShortLevel) },
        { -1L, typeof(UShortLevel) },
        { 5_000_000_000L, typeof(IntLevel) },
        { -1L, typeof(UIntLevel) },
        { (ulong) long.MaxValue + 1, typeof(LongLevel) },
        { -1L, typeof(ULongLevel) },
        { 256L, typeof(Access) },
        { 300L, typeof(ByteLevel?) },
        { -1L, typeof(ByteLevel) },
        { (long) int.MaxValue + 1, typeof(IntLevel) },
        { (long) int.MinValue - 1, typeof(IntLevel) },
        { (long) uint.MaxValue + 1, typeof(UIntLevel) },
        { 4294967295.5, typeof(UIntLevel) },
        { 9223372036854775808.0, typeof(LongLevel) },
        { -9223372036854777856.0, typeof(LongLevel) },
        { 18446744073709551616.0, typeof(ULongLevel) },
        { -1L, typeof(UIntLevel?) },
        { 256L, typeof(ByteLevel?) },

        // What wrapping (int) and (long) casts write for uint and ulong enum values above
        // int.MaxValue and long.MaxValue. Those values don't fit the underlying type.
        { -1L, typeof(Permissions) },
        { (long) int.MinValue, typeof(Permissions) },
        { long.MinValue, typeof(ULongLevel) }
    };

    // Pairings outside the numeric rules stay with the platform's own conversion.
    public static TheoryData<object, Type> NotCovered => new() {
        { "2", typeof(int) },
        { true, typeof(int) },
        { true, typeof(ByteLevel) },
        { 'A', typeof(int) },
        { 65L, typeof(char) },
        { 2L, typeof(string) },
        { 2L, typeof(bool) },
        { 2L, typeof(object) },
        { 2.5, typeof(DateTime) }
    };

    [Theory]
    [MemberData(nameof(Conversions))]
    public void converts_numbers_to_numeric_and_enum_targets(object value, Type targetType, object expected)
    {
        Assert.True(FirestoreNumericConversion.TryConvert(value, targetType, out var result));
        Assert.Equal(expected.GetType(), result.GetType());
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(Overflows))]
    public void throws_when_the_number_cannot_be_represented_by_the_target(object value, Type targetType)
    {
        Assert.Throws<OverflowException>(() => FirestoreNumericConversion.TryConvert(value, targetType, out _));
    }

    [Theory]
    [MemberData(nameof(NotCovered))]
    public void leaves_other_pairings_to_the_caller(object value, Type targetType)
    {
        Assert.False(FirestoreNumericConversion.TryConvert(value, targetType, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void overflow_message_names_the_value_and_the_requested_type()
    {
        var exception = Assert.Throws<OverflowException>(
            () => FirestoreNumericConversion.TryConvert(300L, typeof(ByteLevel), out _));

        Assert.Contains("300", exception.Message);
        Assert.Contains(typeof(ByteLevel).FullName!, exception.Message);
    }

    [Fact]
    public void keeps_negative_zero_for_floating_point_targets()
    {
        Assert.True(FirestoreNumericConversion.TryConvert(-0.0, typeof(float), out var result));
        Assert.True(float.IsNegative((float) result));
    }
}