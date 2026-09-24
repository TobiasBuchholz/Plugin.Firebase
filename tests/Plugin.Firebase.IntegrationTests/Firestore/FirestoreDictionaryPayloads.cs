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