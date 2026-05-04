using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreNullabilityFixture
{
    [Fact]
    public async Task writes_and_reads_null_values_from_supported_payload_shapes()
    {
        var sut = CrossFirebaseFirestore.Current;
        var collection = GetTestingCollection(sut);

        var objectDocument = GetTestingDocument(sut, "object");
        await objectDocument.SetDataAsync(NullableFirestoreItemFactory.CreateNullableItem("object"));

        var dictionaryDocument = GetTestingDocument(sut, "dictionary");
        await dictionaryDocument.SetDataAsync(NullableFirestoreItemFactory.CreateNullableDictionary("dictionary"));

        var tupleDocument = GetTestingDocument(sut, "tuple");
        await tupleDocument.SetDataAsync(NullableFirestoreItemFactory.CreateNullableTuples("tuple"));

        var addedDocument = await collection.AddDocumentAsync(NullableFirestoreItemFactory.CreateNullableItem("added"));

        await FirestoreAssertions.NullableDocumentAsync(objectDocument, "object");
        await FirestoreAssertions.NullableDocumentAsync(dictionaryDocument, "dictionary");
        await FirestoreAssertions.NullableDocumentAsync(tupleDocument, "tuple");
        await FirestoreAssertions.NullableDocumentAsync(addedDocument, "added");
    }
}