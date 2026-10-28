using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

[Preserve(AllMembers = true)]
internal sealed class ListNullDocument : IFirestoreObject
{
    [UsedImplicitly]
    public ListNullDocument()
    {
        // needed for firestore
    }

    public ListNullDocument(IList<object?> values, IList<long?> nullableNumbers, IList<string?> strings)
    {
        Values = values;
        NullableNumbers = nullableNumbers;
        Strings = strings;
    }

    [FirestoreProperty("values")]
    public IList<object?> Values { get; private set; } = null!;

    [FirestoreProperty("nullable_numbers")]
    public IList<long?> NullableNumbers { get; private set; } = null!;

    [FirestoreProperty("strings")]
    public IList<string?> Strings { get; private set; } = null!;
}

[Preserve(AllMembers = true)]
internal sealed class NonNullableListDocument : IFirestoreObject
{
    [UsedImplicitly]
    public NonNullableListDocument()
    {
        // needed for firestore
    }

    [FirestoreProperty("counts")]
    public IList<int> Counts { get; private set; } = null!;
}

[Preserve(AllMembers = true)]
internal sealed class GeoPointListDocument : IFirestoreObject
{
    [UsedImplicitly]
    public GeoPointListDocument()
    {
        // needed for firestore
    }

    public GeoPointListDocument(IList<GeoPoint> locations)
    {
        Locations = locations;
    }

    [FirestoreProperty("locations")]
    public IList<GeoPoint> Locations { get; private set; } = null!;
}