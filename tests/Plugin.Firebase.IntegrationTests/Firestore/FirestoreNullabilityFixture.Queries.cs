using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreNullabilityFixture
{
    [Fact]
    public async Task queries_documents_by_null_field_values()
    {
        var sut = CrossFirebaseFirestore.Current;
        var collection = GetTestingCollection(sut);

        await GetTestingDocument(sut, "null-a").SetDataAsync(NullableFirestoreItemFactory.CreateNullableItem(null));
        await GetTestingDocument(sut, "null-b").SetDataAsync(NullableFirestoreItemFactory.CreateNullableItem(null));
        await GetTestingDocument(sut, "value").SetDataAsync(NullableFirestoreItemFactory.CreateNullableItem("value"));

        var stringFieldSnapshot = await collection
            .WhereEqualsTo(NullableFirestoreItem.QueryMarkerField, null)
            .GetDocumentsAsync<NullableFirestoreItem>();
        var fieldPathSnapshot = await collection
            .WhereEqualsTo(FieldPath.Of([NullableFirestoreItem.QueryMarkerField]), null)
            .GetDocumentsAsync<NullableFirestoreItem>();

        Assert.Equal(
            ["null-a", "null-b"],
                stringFieldSnapshot.Documents.Select(x => FirestoreAssertions.Require(x.Data).Id).OrderBy(x => x)
        );
        Assert.Equal(
            ["null-a", "null-b"],
                fieldPathSnapshot.Documents.Select(x => FirestoreAssertions.Require(x.Data).Id).OrderBy(x => x)
        );
    }
}