using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore
{
    public sealed partial class FirestoreFixture
    {
        [Fact]
        public async Task gets_null_or_empty_dictionary_data_for_missing_and_empty_documents()
        {
            var sut = CrossFirebaseFirestore.Current;
            var missingDocument = GetTestingDocument(sut, "missing-raw-data");
            var emptyDocument = GetTestingDocument(sut, "empty-raw-data");

            Assert.Null((await missingDocument.GetDocumentSnapshotAsync<Dictionary<string, object?>>()).Data);
            Assert.Null((await missingDocument.GetDocumentSnapshotAsync<object>()).Data);

            await emptyDocument.SetDataAsync(new Dictionary<object, object?> { { "temporary", "value" } });
            await emptyDocument.UpdateDataAsync(("temporary", FieldValue.Delete()));

            var dictionarySnapshot = await emptyDocument.GetDocumentSnapshotAsync<Dictionary<string, object?>>();
            Assert.NotNull(dictionarySnapshot.Data);
            Assert.Empty(dictionarySnapshot.Data!);

            var objectSnapshot = await emptyDocument.GetDocumentSnapshotAsync<object>();
            Assert.Empty(Assert.IsAssignableFrom<IDictionary<string, object?>>(objectSnapshot.Data!));
        }

        [Fact]
        public async Task reads_null_entries_inside_firestore_lists()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "list-null-values");
            await document.SetDataAsync(new ListNullDocument(
                values: ["first", null, "third"],
                nullableNumbers: [1L, null, 3L],
                strings: ["first", null, "last"]));

            var data = (await document.GetDocumentSnapshotAsync<ListNullDocument>()).Data!;

            Assert.Equal("first", data.Values[0]);
            Assert.Null(data.Values[1]);
            Assert.Equal("third", data.Values[2]);
            Assert.Equal([1L, null, 3L], data.NullableNumbers);
            // a typed string list takes the conversion path that used to turn null into "" on Android (#706)
            Assert.Equal(["first", null, "last"], data.Strings);
        }

        [Fact]
        public async Task rejects_null_entries_in_lists_of_non_nullable_value_types()
        {
            var sut = CrossFirebaseFirestore.Current;
            var controlDocument = GetTestingDocument(sut, "list-non-nullable-values");
            var document = GetTestingDocument(sut, "list-null-non-nullable-values");
            await controlDocument.SetDataAsync(new Dictionary<object, object?> {
                { "counts", new object?[] { 1L, 2L, 3L } }
            });
            await document.SetDataAsync(new Dictionary<object, object?> {
                { "counts", new object?[] { 1L, null, 3L } }
            });

            // the same model reads an array without null entries, so only the null entry makes the read below fail
            Assert.Equal([1, 2, 3], (await controlDocument.GetDocumentSnapshotAsync<NonNullableListDocument>()).Data!.Counts);

            var snapshot = await document.GetDocumentSnapshotAsync<NonNullableListDocument>();

            // List<int> rejects the null entry when snapshot.Data converts the document; it is not replaced with 0
            // or dropped. "item" is the parameter name List<T>'s IList.Add reports on both platforms.
            var exception = Assert.Throws<ArgumentNullException>(() => snapshot.Data);
            Assert.Equal("item", exception.ParamName);
        }

        [Fact]
        public async Task writes_geopoint_values_inside_firestore_lists()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "geopoint-list-values");
            var expected = new[] {
                new GeoPoint(10.5, 20.25),
                new GeoPoint(-33.875, 151.2)
            };

            await document.SetDataAsync(new GeoPointListDocument(expected));

            var result = (await document.GetDocumentSnapshotAsync<GeoPointListDocument>()).Data!;
            Assert.Equal(expected[0].Latitude, result.Locations[0].Latitude);
            Assert.Equal(expected[0].Longitude, result.Locations[0].Longitude);
            Assert.Equal(expected[1].Latitude, result.Locations[1].Latitude);
            Assert.Equal(expected[1].Longitude, result.Locations[1].Longitude);
        }
    }
}