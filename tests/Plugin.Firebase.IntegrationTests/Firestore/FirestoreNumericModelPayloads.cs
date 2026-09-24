using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

[Preserve(AllMembers = true)]
internal sealed class NumericWidthsDocument : IFirestoreObject
{
    [FirestoreProperty("byte_value")]
    public byte ByteValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("sbyte_value")]
    public sbyte SbyteValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("short_value")]
    public short ShortValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("ushort_value")]
    public ushort UshortValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("int_value")]
    public int IntValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("uint_value")]
    public uint UintValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("long_value")]
    public long LongValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("ulong_value")]
    public ulong UlongValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("float_value")]
    public float FloatValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("whole_float_value")]
    public float WholeFloatValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("double_value")]
    public double DoubleValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("whole_double_value")]
    public double WholeDoubleValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("decimal_from_whole_value")]
    public decimal DecimalFromWholeValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("decimal_from_fraction_value")]
    public decimal DecimalFromFractionValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("char_value")]
    public char CharValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("byte_list_value")]
    public IList<byte>? ByteListValue { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class NullableNumericDocument : IFirestoreObject
{
    [FirestoreProperty("nullable_byte_value")]
    public byte? NullableByteValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("nullable_int_value")]
    public int? NullableIntValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("nullable_float_value")]
    public float? NullableFloatValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("nullable_double_value")]
    public double? NullableDoubleValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("absent_value")]
    public double? AbsentValue { get; [UsedImplicitly] private set; }
}

internal enum NumericRating : byte
{
    Unrated,
    Low,
    High
}

[Preserve(AllMembers = true)]
internal sealed class EnumModelDocument : IFirestoreObject
{
    [FirestoreProperty("poke_type_value")]
    public PokeType PokeTypeValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("rating_value")]
    public NumericRating RatingValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("rating_from_double_value")]
    public NumericRating RatingFromDoubleValue { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class CultureConversionDocument : IFirestoreObject
{
    [FirestoreProperty("double_from_text_value")]
    public double DoubleFromTextValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("text_from_double_value")]
    public string? TextFromDoubleValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("texts_from_doubles_value")]
    public IList<string>? TextsFromDoublesValue { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class NumericBoundariesDocument : IFirestoreObject
{
    [FirestoreProperty("byte_min")]
    public byte ByteMin { get; [UsedImplicitly] private set; }

    [FirestoreProperty("byte_max")]
    public byte ByteMax { get; [UsedImplicitly] private set; }

    [FirestoreProperty("sbyte_min")]
    public sbyte SByteMin { get; [UsedImplicitly] private set; }

    [FirestoreProperty("sbyte_max")]
    public sbyte SByteMax { get; [UsedImplicitly] private set; }

    [FirestoreProperty("short_min")]
    public short ShortMin { get; [UsedImplicitly] private set; }

    [FirestoreProperty("short_max")]
    public short ShortMax { get; [UsedImplicitly] private set; }

    [FirestoreProperty("ushort_min")]
    public ushort UShortMin { get; [UsedImplicitly] private set; }

    [FirestoreProperty("ushort_max")]
    public ushort UShortMax { get; [UsedImplicitly] private set; }

    [FirestoreProperty("int_min")]
    public int IntMin { get; [UsedImplicitly] private set; }

    [FirestoreProperty("int_max")]
    public int IntMax { get; [UsedImplicitly] private set; }

    [FirestoreProperty("uint_min")]
    public uint UIntMin { get; [UsedImplicitly] private set; }

    [FirestoreProperty("uint_max")]
    public uint UIntMax { get; [UsedImplicitly] private set; }

    [FirestoreProperty("long_min")]
    public long LongMin { get; [UsedImplicitly] private set; }

    [FirestoreProperty("long_max")]
    public long LongMax { get; [UsedImplicitly] private set; }

    [FirestoreProperty("ulong_min")]
    public ulong ULongMin { get; [UsedImplicitly] private set; }

    [FirestoreProperty("ulong_max")]
    public ulong ULongMax { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class NullableDecimalDocument : IFirestoreObject
{
    [FirestoreProperty("nullable_present")]
    public decimal? NullablePresent { get; [UsedImplicitly] private set; }

    [FirestoreProperty("nullable_missing")]
    public decimal? NullableMissing { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class RoundedIntegralsDocument : IFirestoreObject
{
    [FirestoreProperty("round_down")]
    public long RoundDown { get; [UsedImplicitly] private set; }

    [FirestoreProperty("round_up")]
    public long RoundUp { get; [UsedImplicitly] private set; }

    [FirestoreProperty("half_to_even_down")]
    public long HalfToEvenDown { get; [UsedImplicitly] private set; }

    [FirestoreProperty("half_to_even_up")]
    public long HalfToEvenUp { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class ByteWithinRangeDocument : IFirestoreObject
{
    [FirestoreProperty("within_byte")]
    public byte Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class ByteRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_byte")]
    public byte Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class SByteRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_sbyte")]
    public sbyte Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class ShortRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_short")]
    public short Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class UShortRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_ushort")]
    public ushort Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class IntRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_int")]
    public int Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class NullableIntRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_int")]
    public int? Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class UIntRangeDocument : IFirestoreObject
{
    [FirestoreProperty("below_unsigned")]
    public uint Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class ULongRangeDocument : IFirestoreObject
{
    [FirestoreProperty("below_unsigned")]
    public ulong Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class CharRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_char")]
    public char Value { get; [UsedImplicitly] private set; }
}

[Preserve(AllMembers = true)]
internal sealed class FractionalCharDocument : IFirestoreObject
{
    [FirestoreProperty("fractional")]
    public char Value { get; [UsedImplicitly] private set; }
}