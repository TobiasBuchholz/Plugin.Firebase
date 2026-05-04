using JetBrains.Annotations;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed class NullableFirestoreItem : IFirestoreObject
{
    internal const string NullableStringField = "nullable_string";
    internal const string NullableNumberField = "nullable_number";
    internal const string NullableMapField = "nullable_map";
    internal const string NullableListField = "nullable_list";
    internal const string QueryMarkerField = "query_marker";

    [Preserve]
    public NullableFirestoreItem()
    {
        // needed for firestore
    }

    public NullableFirestoreItem(
        string? nullableString,
        long? nullableNumber,
        Dictionary<string, object?>? nullableMap,
        List<object?>? nullableList,
        string? queryMarker
    )
    {
        NullableString = nullableString;
        NullableNumber = nullableNumber;
        NullableMap = nullableMap;
        NullableList = nullableList;
        QueryMarker = queryMarker;
    }

    [FirestoreDocumentId]
    public string? Id { get; [UsedImplicitly] private set; }

    [FirestoreProperty(NullableStringField)]
    public string? NullableString { get; set; }

    [FirestoreProperty(NullableNumberField)]
    public long? NullableNumber { get; set; }

    [FirestoreProperty(NullableMapField)]
    public Dictionary<string, object?>? NullableMap { get; set; }

    [FirestoreProperty(NullableListField)]
    public List<object?>? NullableList { get; set; }

    [FirestoreProperty(QueryMarkerField)]
    public string? QueryMarker { get; set; }
}