using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task gets_document_data_as_typed_nested_dictionaries()
    {
        var sut = CrossFirebaseFirestore.Current;

        var objectMapDocument = GetTestingDocument(sut, "typed-nested-object-map");
        await objectMapDocument.SetDataAsync(new Dictionary<object, object?> {
            {
                "outer",
                new Dictionary<object, object?> {
                    { "name", "nested" },
                    { "count", 3L },
                    { "active", true },
                    { "empty", new Dictionary<object, object?>() },
                    { "inner", new Dictionary<object, object?> { { "value", "deep" } } }
                }
            }
        });
        var objectMaps = (await objectMapDocument.GetDocumentSnapshotAsync<Dictionary<string, Dictionary<string, object?>>>()).Data!;
        var outerObjectMap = objectMaps["outer"];
        Assert.Equal("nested", outerObjectMap["name"]);
        Assert.Equal(3L, Convert.ToInt64(outerObjectMap["count"]));
        Assert.True((bool) outerObjectMap["active"]!);
        Assert.Empty(Assert.IsAssignableFrom<IDictionary<string, object?>>(outerObjectMap["empty"]));
        Assert.Equal(
            "deep",
            Assert.IsAssignableFrom<IDictionary<string, object?>>(outerObjectMap["inner"])["value"]);

        var interfaceMapDocument = GetTestingDocument(sut, "typed-nested-interface-map");
        await interfaceMapDocument.SetDataAsync(new Dictionary<object, object?> {
            {
                "outer",
                new Dictionary<object, object?> {
                    { "label", "interface" },
                    { "nullable", null }
                }
            }
        });
        var interfaceMaps = (await interfaceMapDocument.GetDocumentSnapshotAsync<Dictionary<string, IDictionary<string, object?>>>()).Data!;
        Assert.Equal("interface", interfaceMaps["outer"]["label"]);
        Assert.Null(interfaceMaps["outer"]["nullable"]);

        var longMapDocument = GetTestingDocument(sut, "typed-nested-long-map");
        await longMapDocument.SetDataAsync(new Dictionary<object, object?> {
            {
                "outer",
                new Dictionary<object, object?> {
                    { "one", 1L },
                    { "two", 2 }
                }
            }
        });
        var longMaps = (await longMapDocument.GetDocumentSnapshotAsync<Dictionary<string, Dictionary<string, long>>>()).Data!;
        Assert.Equal(1L, longMaps["outer"]["one"]);
        Assert.Equal(2L, longMaps["outer"]["two"]);
    }
}