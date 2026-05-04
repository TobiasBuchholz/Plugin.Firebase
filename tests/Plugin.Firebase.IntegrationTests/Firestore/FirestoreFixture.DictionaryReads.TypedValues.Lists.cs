using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task gets_document_data_as_typed_list_dictionaries()
    {
        var sut = CrossFirebaseFirestore.Current;

        var longListDocument = GetTestingDocument(sut, "typed-long-list-map");
        await longListDocument.SetDataAsync(new Dictionary<object, object?> {
            { "first", new[] { 1L, 2L } },
            { "second", new[] { 3, 4 } }
        });
        var longLists = (await longListDocument.GetDocumentSnapshotAsync<Dictionary<string, IList<long>>>()).Data!;
        Assert.Equal([1L, 2L], longLists["first"]);
        Assert.Equal([3L, 4L], longLists["second"]);

        var nullableListDocument = GetTestingDocument(sut, "typed-nullable-list-map");
        await nullableListDocument.SetDataAsync(new Dictionary<object, object?> {
            { "values", new object?[] { 1L, null, 3L } }
        });
        var nullableLists = (await nullableListDocument.GetDocumentSnapshotAsync<Dictionary<string, IList<long?>>>()).Data!;
        Assert.Equal([1L, null, 3L], nullableLists["values"]);

        var objectListDocument = GetTestingDocument(sut, "typed-object-list-map");
        await objectListDocument.SetDataAsync(new Dictionary<object, object?> {
            {
                "values",
                new object?[] {
                    null,
                    "text",
                    5L,
                    new Dictionary<object, object?> { { "name", "map" } },
                    new Dictionary<object, object?> {
                        { "active", true },
                        { "nullable", null }
                    }
                }
            },
            { "empty", Array.Empty<object?>() }
        });
        var objectLists = (await objectListDocument.GetDocumentSnapshotAsync<Dictionary<string, IList<object?>>>()).Data!;
        Assert.Empty(objectLists["empty"]);

        var values = objectLists["values"];
        Assert.Null(values[0]);
        Assert.Equal("text", values[1]);
        Assert.Equal(5L, Convert.ToInt64(values[2]));
        Assert.Equal("map", Assert.IsAssignableFrom<IDictionary<string, object?>>(values[3])["name"]);

        var nestedMap = Assert.IsAssignableFrom<IDictionary<string, object?>>(values[4]);
        Assert.True((bool) nestedMap["active"]!);
        Assert.Null(nestedMap["nullable"]);
    }
}