using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

[Preserve(AllMembers = true)]
internal sealed class TypedMapValuesDocument : IFirestoreObject
{
    [UsedImplicitly]
    public TypedMapValuesDocument()
    {
        // needed for firestore
    }

    public TypedMapValuesDocument(
        Dictionary<string, bool> booleanMaps,
        Dictionary<string, DateTimeOffset> dateMaps)
    {
        BooleanMaps = booleanMaps;
        DateMaps = dateMaps;
    }

    [FirestoreProperty("boolean_maps")]
    public Dictionary<string, bool> BooleanMaps { get; private set; } = null!;

    [FirestoreProperty("date_maps")]
    public Dictionary<string, DateTimeOffset> DateMaps { get; private set; } = null!;
}

[Preserve(AllMembers = true)]
internal sealed class DictionaryObjectValuesDocument : IFirestoreObject
{
    [UsedImplicitly]
    public DictionaryObjectValuesDocument()
    {
        // needed for firestore
    }

    public DictionaryObjectValuesDocument(Dictionary<string, object?> values)
    {
        Values = values;
    }

    [FirestoreProperty("values")]
    public Dictionary<string, object?> Values { get; private set; } = null!;
}

[Preserve(AllMembers = true)]
internal sealed class EnumDictionaryDocument : IFirestoreObject
{
    [UsedImplicitly]
    public EnumDictionaryDocument()
    {
        // needed for firestore
    }

    public EnumDictionaryDocument(Dictionary<string, PokeType> values)
    {
        Values = values;
    }

    [FirestoreProperty("values")]
    public Dictionary<string, PokeType> Values { get; private set; } = null!;
}

[Preserve(AllMembers = true)]
internal sealed class AndroidNumericCollectionsDocument : IFirestoreObject
{
    [FirestoreProperty("counts")]
    public Dictionary<string, int> Counts { get; private set; } = null!;

    [FirestoreProperty("nullable_counts")]
    public IList<int?> NullableCounts { get; private set; } = null!;

    [FirestoreProperty("types")]
    public Dictionary<string, PokeType> Types { get; private set; } = null!;
}

[Preserve(AllMembers = true)]
internal sealed class NestedShortDictionaryDocument : IFirestoreObject
{
    [UsedImplicitly]
    public NestedShortDictionaryDocument()
    {
        // needed for firestore
    }

    public NestedShortDictionaryDocument(Dictionary<string, Dictionary<string, short>> values)
    {
        Values = values;
    }

    [FirestoreProperty("values")]
    public Dictionary<string, Dictionary<string, short>> Values { get; [UsedImplicitly] private set; } = null!;
}

[Preserve(AllMembers = true)]
internal sealed class DictionaryContainer : IFirestoreObject
{
    [UsedImplicitly]
    public DictionaryContainer()
    {
        // needed for firestore
    }

    public DictionaryContainer(
        Dictionary<string, object?> metadata,
        IDictionary<string, long> scores,
        Dictionary<string, bool> flags,
        Dictionary<string, IList<object?>> mixedLists,
        Dictionary<string, Dictionary<string, object?>> nested
    )
    {
        Metadata = metadata;
        Scores = scores;
        Flags = flags;
        MixedLists = mixedLists;
        Nested = nested;
    }

    [FirestoreDocumentId]
    public string Id { get; private set; } = null!;

    [FirestoreProperty("metadata")]
    public Dictionary<string, object?> Metadata { get; private set; } = null!;

    [FirestoreProperty("scores")]
    public IDictionary<string, long> Scores { get; private set; } = null!;

    [FirestoreProperty("flags")]
    public Dictionary<string, bool> Flags { get; private set; } = null!;

    [FirestoreProperty("mixed_lists")]
    public Dictionary<string, IList<object?>> MixedLists { get; private set; } = null!;

    [FirestoreProperty("nested")]
    public Dictionary<string, Dictionary<string, object?>> Nested { get; private set; } = null!;
}

[Preserve(AllMembers = true)]
internal sealed class NumericBoundariesDocument : IFirestoreObject
{
    [FirestoreProperty("byte_min")]
    public byte ByteMin { get; private set; }

    [FirestoreProperty("byte_max")]
    public byte ByteMax { get; private set; }

    [FirestoreProperty("sbyte_min")]
    public sbyte SByteMin { get; private set; }

    [FirestoreProperty("sbyte_max")]
    public sbyte SByteMax { get; private set; }

    [FirestoreProperty("short_min")]
    public short ShortMin { get; private set; }

    [FirestoreProperty("short_max")]
    public short ShortMax { get; private set; }

    [FirestoreProperty("ushort_min")]
    public ushort UShortMin { get; private set; }

    [FirestoreProperty("ushort_max")]
    public ushort UShortMax { get; private set; }

    [FirestoreProperty("int_min")]
    public int IntMin { get; private set; }

    [FirestoreProperty("int_max")]
    public int IntMax { get; private set; }

    [FirestoreProperty("uint_min")]
    public uint UIntMin { get; private set; }

    [FirestoreProperty("uint_max")]
    public uint UIntMax { get; private set; }

    [FirestoreProperty("long_min")]
    public long LongMin { get; private set; }

    [FirestoreProperty("long_max")]
    public long LongMax { get; private set; }

    [FirestoreProperty("ulong_min")]
    public ulong ULongMin { get; private set; }

    [FirestoreProperty("ulong_max")]
    public ulong ULongMax { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class DecimalValuesDocument : IFirestoreObject
{
    [FirestoreProperty("integral")]
    public decimal Integral { get; private set; }

    [FirestoreProperty("fractional")]
    public decimal Fractional { get; private set; }

    [FirestoreProperty("nullable_present")]
    public decimal? NullablePresent { get; private set; }

    [FirestoreProperty("nullable_missing")]
    public decimal? NullableMissing { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class CharValuesDocument : IFirestoreObject
{
    [FirestoreProperty("letter")]
    public char Letter { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class RoundedIntegralsDocument : IFirestoreObject
{
    [FirestoreProperty("round_down")]
    public long RoundDown { get; private set; }

    [FirestoreProperty("round_up")]
    public long RoundUp { get; private set; }

    [FirestoreProperty("half_to_even_down")]
    public long HalfToEvenDown { get; private set; }

    [FirestoreProperty("half_to_even_up")]
    public long HalfToEvenUp { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class ByteWithinRangeDocument : IFirestoreObject
{
    [FirestoreProperty("within_byte")]
    public byte Value { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class ByteRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_byte")]
    public byte Value { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class SByteRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_sbyte")]
    public sbyte Value { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class ShortRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_short")]
    public short Value { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class UShortRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_ushort")]
    public ushort Value { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class IntRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_int")]
    public int Value { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class NullableIntRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_int")]
    public int? Value { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class UIntRangeDocument : IFirestoreObject
{
    [FirestoreProperty("below_unsigned")]
    public uint Value { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class ULongRangeDocument : IFirestoreObject
{
    [FirestoreProperty("below_unsigned")]
    public ulong Value { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class CharRangeDocument : IFirestoreObject
{
    [FirestoreProperty("above_char")]
    public char Value { get; private set; }
}

[Preserve(AllMembers = true)]
internal sealed class FractionalCharDocument : IFirestoreObject
{
    [FirestoreProperty("fractional")]
    public char Value { get; private set; }
}