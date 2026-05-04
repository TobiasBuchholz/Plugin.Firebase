using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreNullabilityFixture
{
    [Fact]
    public async Task updates_null_values_from_document_batch_and_transaction_writes()
    {
        var sut = CrossFirebaseFirestore.Current;

        var documentUpdate = GetTestingDocument(sut, "document-update");
        await documentUpdate.SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("document-update-seed"));
        await documentUpdate.UpdateDataAsync(NullableFirestoreItemFactory.CreateNullUpdateTuples("document-update"));

        var batchSet = GetTestingDocument(sut, "batch-set");
        var batchUpdate = GetTestingDocument(sut, "batch-update");
        await batchUpdate.SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("batch-update-seed"));

        var batch = sut.CreateBatch();
        batch.SetData(batchSet, NullableFirestoreItemFactory.CreateNullableDictionary("batch-set"));
        batch.UpdateData(
            batchUpdate,
            NullableFirestoreItemFactory.CreateNullUpdate("batch-update")
        );
        await batch.CommitAsync();

        var transactionDocument = GetTestingDocument(sut, "transaction");
        await transactionDocument.SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("transaction-seed"));
        var transactionResult = await sut.RunTransactionAsync<string?>(transaction => {
            transaction.UpdateData(
                transactionDocument,
                NullableFirestoreItemFactory.CreateNullUpdateTuples(null)
            );
            return null;
        });

        Assert.Null(transactionResult);
        await FirestoreAssertions.NullableDocumentAsync(documentUpdate, "document-update");
        await FirestoreAssertions.NullableDocumentAsync(batchSet, "batch-set");
        await FirestoreAssertions.NullableDocumentAsync(batchUpdate, "batch-update");
        await FirestoreAssertions.NullableDocumentAsync(transactionDocument, null);
    }

    [Fact]
    public async Task updates_nested_object_dictionary_maps_from_document_batch_and_transaction_writes()
    {
        var sut = CrossFirebaseFirestore.Current;

        var documentUpdate = GetTestingDocument(sut, "issue-482-document-update");
        await documentUpdate.SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("issue-482-document-update-seed"));
        await documentUpdate.UpdateDataAsync(NullableFirestoreItemFactory.CreateIssue482NestedMapUpdate("document-update"));

        var batchUpdate = GetTestingDocument(sut, "issue-482-batch-update");
        await batchUpdate.SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("issue-482-batch-update-seed"));

        var batch = sut.CreateBatch();
        batch.UpdateData(batchUpdate, NullableFirestoreItemFactory.CreateIssue482NestedMapUpdate("batch-update"));
        await batch.CommitAsync();

        var transactionUpdate = GetTestingDocument(sut, "issue-482-transaction-update");
        await transactionUpdate.SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("issue-482-transaction-update-seed"));
        await sut.RunTransactionAsync(transaction => {
            transaction.UpdateData(transactionUpdate, NullableFirestoreItemFactory.CreateIssue482NestedMapUpdate("transaction-update"));
            return true;
        });

        await FirestoreAssertions.Issue482NestedMapAsync(documentUpdate, "document-update");
        await FirestoreAssertions.Issue482NestedMapAsync(batchUpdate, "batch-update");
        await FirestoreAssertions.Issue482NestedMapAsync(transactionUpdate, "transaction-update");
    }

    [Fact]
    public async Task applies_null_array_transforms()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "array-transforms");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { NullableFirestoreItem.NullableListField, new List<object?> { "existing" } },
            { NullableFirestoreItem.QueryMarkerField, "array-transforms" }
        });

        await document.UpdateDataAsync((NullableFirestoreItem.NullableListField, FieldValue.ArrayUnion(null, "added")));

        var afterUnion = FirestoreAssertions.Require(
            (await document.GetDocumentSnapshotAsync<NullableFirestoreItem>()).Data?.NullableList
        );
        Assert.Equal(["existing", null, "added"], afterUnion);

        await document.UpdateDataAsync((NullableFirestoreItem.NullableListField, FieldValue.ArrayRemove([null])));

        var afterRemove = FirestoreAssertions.Require(
            (await document.GetDocumentSnapshotAsync<NullableFirestoreItem>()).Data?.NullableList
        );
        Assert.Equal(["existing", "added"], afterRemove);
    }
}