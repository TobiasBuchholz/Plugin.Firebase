using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

[Preserve(AllMembers = true)]
internal sealed class NumericWidthsDocument : IFirestoreObject
{
    [UsedImplicitly]
    public NumericWidthsDocument()
    {
        // needed for firestore
    }

    public NumericWidthsDocument(
        byte byteValue,
        sbyte sbyteValue,
        short shortValue,
        ushort ushortValue,
        int intValue,
        uint uintValue,
        long longValue,
        ulong ulongValue,
        float floatValue,
        float wholeFloatValue,
        double doubleValue,
        double wholeDoubleValue,
        decimal decimalFromWholeValue,
        decimal decimalFromFractionValue,
        char charValue,
        IList<byte>? byteListValue
    )
    {
        ByteValue = byteValue;
        SbyteValue = sbyteValue;
        ShortValue = shortValue;
        UshortValue = ushortValue;
        IntValue = intValue;
        UintValue = uintValue;
        LongValue = longValue;
        UlongValue = ulongValue;
        FloatValue = floatValue;
        WholeFloatValue = wholeFloatValue;
        DoubleValue = doubleValue;
        WholeDoubleValue = wholeDoubleValue;
        DecimalFromWholeValue = decimalFromWholeValue;
        DecimalFromFractionValue = decimalFromFractionValue;
        CharValue = charValue;
        ByteListValue = byteListValue;
    }

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

internal enum SbyteRating : sbyte
{
    Negative = -1,
    Positive = 1
}

internal enum ShortRating : short
{
    Negative = -300,
    Positive = 300
}

internal enum UshortRating : ushort
{
    Low = 400,
    High = 600
}

internal enum UintRating : uint
{
    Low = 3_000_000_000,
    High = 4_000_000_000
}

internal enum LongRating : long
{
    Low = -5_000_000_000,
    High = 5_000_000_000
}

internal enum UlongRating : ulong
{
    High = 9_000_000_000_000_000_000
}

[Preserve(AllMembers = true)]
internal sealed class EnumWidthsDocument : IFirestoreObject
{
    [UsedImplicitly]
    public EnumWidthsDocument()
    {
        // needed for firestore
    }

    public EnumWidthsDocument(
        NumericRating byteEnum,
        SbyteRating sbyteEnum,
        ShortRating shortEnum,
        UshortRating ushortEnum,
        PokeType intEnum,
        UintRating uintEnum,
        LongRating longEnum,
        UlongRating ulongEnum
    )
    {
        ByteEnum = byteEnum;
        SbyteEnum = sbyteEnum;
        ShortEnum = shortEnum;
        UshortEnum = ushortEnum;
        IntEnum = intEnum;
        UintEnum = uintEnum;
        LongEnum = longEnum;
        UlongEnum = ulongEnum;
    }

    [FirestoreProperty("byte_enum")]
    public NumericRating ByteEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("sbyte_enum")]
    public SbyteRating SbyteEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("short_enum")]
    public ShortRating ShortEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("ushort_enum")]
    public UshortRating UshortEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("int_enum")]
    public PokeType IntEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("uint_enum")]
    public UintRating UintEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("long_enum")]
    public LongRating LongEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("ulong_enum")]
    public UlongRating UlongEnum { get; [UsedImplicitly] private set; }
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