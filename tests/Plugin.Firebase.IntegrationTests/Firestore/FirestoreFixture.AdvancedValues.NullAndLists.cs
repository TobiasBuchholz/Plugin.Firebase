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

        [IosFact]
        public async Task reads_null_entries_inside_firestore_lists()
        {
            var sut = CrossFirebaseFirestore.Current;
            var document = GetTestingDocument(sut, "list-null-values");
            await document.SetDataAsync(new ListNullDocument(
                values: ["first", null, "third"],
                nullableNumbers: [1L, null, 3L]));

            var snapshot = await document.GetDocumentSnapshotAsync<ListNullDocument>();

            Assert.Equal("first", snapshot.Data!.Values[0]);
            Assert.Null(snapshot.Data!.Values[1]);
            Assert.Equal("third", snapshot.Data!.Values[2]);
            Assert.Equal([1L, null, 3L], snapshot.Data!.NullableNumbers);
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