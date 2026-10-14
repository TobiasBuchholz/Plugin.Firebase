using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

[Preserve(AllMembers = true)]
internal sealed class NumericWritesDocument : IFirestoreObject
{
    public static NumericWritesDocument CreateMin() => new() {
        ByteValue = byte.MinValue,
        SbyteValue = sbyte.MinValue,
        ShortValue = short.MinValue,
        UshortValue = ushort.MinValue,
        IntValue = int.MinValue,
        UintValue = uint.MinValue,
        LongValue = long.MinValue,
        UlongValue = ulong.MinValue,
        FloatValue = float.MinValue,
        DoubleValue = double.MinValue
    };

    public static NumericWritesDocument CreateMax() => new() {
        ByteValue = byte.MaxValue,
        SbyteValue = sbyte.MaxValue,
        ShortValue = short.MaxValue,
        UshortValue = ushort.MaxValue,
        IntValue = int.MaxValue,
        UintValue = uint.MaxValue,
        LongValue = long.MaxValue,
        // the largest ulong Firestore can store
        UlongValue = long.MaxValue,
        FloatValue = float.MaxValue,
        DoubleValue = double.MaxValue
    };

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

    [FirestoreProperty("double_value")]
    public double DoubleValue { get; [UsedImplicitly] private set; }

    // Firestore stores every integral value as an int64 and float as a double
    public IReadOnlyDictionary<string, object?> ToWireValues() => new Dictionary<string, object?> {
        { "byte_value", (long) ByteValue },
        { "sbyte_value", (long) SbyteValue },
        { "short_value", (long) ShortValue },
        { "ushort_value", (long) UshortValue },
        { "int_value", (long) IntValue },
        { "uint_value", (long) UintValue },
        { "long_value", LongValue },
        { "ulong_value", (long) UlongValue },
        { "float_value", (double) FloatValue },
        { "double_value", DoubleValue }
    };
}

[Preserve(AllMembers = true)]
internal sealed class NullableNumericWritesDocument : IFirestoreObject
{
    public static NullableNumericWritesDocument CreateWithValues() => new() {
        NullableByteValue = byte.MaxValue,
        NullableUintValue = uint.MaxValue,
        NullableUlongValue = long.MaxValue,
        NullableFloatValue = 1.5f,
        NullableEnumValue = IntRating.Min
    };

    [FirestoreProperty("nullable_byte_value")]
    public byte? NullableByteValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("nullable_uint_value")]
    public uint? NullableUintValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("nullable_ulong_value")]
    public ulong? NullableUlongValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("nullable_float_value")]
    public float? NullableFloatValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("nullable_enum_value")]
    public IntRating? NullableEnumValue { get; [UsedImplicitly] private set; }

    [FirestoreProperty("nullable_int_value")]
    public int? NullableIntValue { get; [UsedImplicitly] private set; }

    public IReadOnlyDictionary<string, object?> ToWireValues() => new Dictionary<string, object?> {
        { "nullable_byte_value", (long?) NullableByteValue },
        { "nullable_uint_value", (long?) NullableUintValue },
        { "nullable_ulong_value", (long?) NullableUlongValue },
        { "nullable_float_value", (double?) NullableFloatValue },
        { "nullable_enum_value", (long?) NullableEnumValue },
        { "nullable_int_value", (long?) NullableIntValue }
    };
}

internal enum ByteRating : byte { Min = byte.MinValue, Max = byte.MaxValue }

internal enum SbyteRating : sbyte { Min = sbyte.MinValue, Max = sbyte.MaxValue }

internal enum ShortRating : short { Min = short.MinValue, Max = short.MaxValue }

internal enum UshortRating : ushort { Min = ushort.MinValue, Max = ushort.MaxValue }

internal enum IntRating { Min = int.MinValue, Max = int.MaxValue }

internal enum UintRating : uint { Min = uint.MinValue, Max = uint.MaxValue }

internal enum LongRating : long { Min = long.MinValue, Max = long.MaxValue }

// Max is the largest ulong Firestore can store; AboveInt64Max has no Firestore representation
internal enum UlongRating : ulong { Min = ulong.MinValue, Max = long.MaxValue, AboveInt64Max = (ulong) long.MaxValue + 1 }

[Preserve(AllMembers = true)]
internal sealed class EnumWidthsDocument : IFirestoreObject
{
    public static EnumWidthsDocument CreateMin() => new() {
        ByteEnum = ByteRating.Min,
        SbyteEnum = SbyteRating.Min,
        ShortEnum = ShortRating.Min,
        UshortEnum = UshortRating.Min,
        IntEnum = IntRating.Min,
        UintEnum = UintRating.Min,
        LongEnum = LongRating.Min,
        UlongEnum = UlongRating.Min
    };

    public static EnumWidthsDocument CreateMax() => new() {
        ByteEnum = ByteRating.Max,
        SbyteEnum = SbyteRating.Max,
        ShortEnum = ShortRating.Max,
        UshortEnum = UshortRating.Max,
        IntEnum = IntRating.Max,
        UintEnum = UintRating.Max,
        LongEnum = LongRating.Max,
        UlongEnum = UlongRating.Max
    };

    [FirestoreProperty("byte_enum")]
    public ByteRating ByteEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("sbyte_enum")]
    public SbyteRating SbyteEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("short_enum")]
    public ShortRating ShortEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("ushort_enum")]
    public UshortRating UshortEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("int_enum")]
    public IntRating IntEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("uint_enum")]
    public UintRating UintEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("long_enum")]
    public LongRating LongEnum { get; [UsedImplicitly] private set; }

    [FirestoreProperty("ulong_enum")]
    public UlongRating UlongEnum { get; [UsedImplicitly] private set; }

    // the same values as update fields, which Android converts on a different path than set
    public Dictionary<object, object?> ToFieldValues() => new() {
        { "byte_enum", ByteEnum },
        { "sbyte_enum", SbyteEnum },
        { "short_enum", ShortEnum },
        { "ushort_enum", UshortEnum },
        { "int_enum", IntEnum },
        { "uint_enum", UintEnum },
        { "long_enum", LongEnum },
        { "ulong_enum", UlongEnum }
    };

    // an enum is stored as its underlying value, widened to an int64 without wrapping
    public IReadOnlyDictionary<string, object?> ToWireValues() => new Dictionary<string, object?> {
        { "byte_enum", (long) ByteEnum },
        { "sbyte_enum", (long) SbyteEnum },
        { "short_enum", (long) ShortEnum },
        { "ushort_enum", (long) UshortEnum },
        { "int_enum", (long) IntEnum },
        { "uint_enum", (long) UintEnum },
        { "long_enum", (long) LongEnum },
        { "ulong_enum", (long) UlongEnum }
    };
}

[Preserve(AllMembers = true)]
internal sealed class DecimalPriceDocument : IFirestoreObject
{
    [FirestoreProperty("price")]
    public decimal Price { get; [UsedImplicitly] private set; } = 19.99m;
}