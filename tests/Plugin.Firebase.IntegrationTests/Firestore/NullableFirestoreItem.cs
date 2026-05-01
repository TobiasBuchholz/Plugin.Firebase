#nullable enable

using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed class NullableFirestoreItem : IFirestoreObject
    {
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
        public string? Id { get; private set; }

        [FirestoreProperty("nullable_string")]
        public string? NullableString { get; set; }

        [FirestoreProperty("nullable_number")]
        public long? NullableNumber { get; set; }

        [FirestoreProperty("nullable_map")]
        public Dictionary<string, object?>? NullableMap { get; set; }

        [FirestoreProperty("nullable_list")]
        public List<object?>? NullableList { get; set; }

        [FirestoreProperty("query_marker")]
        public string? QueryMarker { get; set; }
    }
}