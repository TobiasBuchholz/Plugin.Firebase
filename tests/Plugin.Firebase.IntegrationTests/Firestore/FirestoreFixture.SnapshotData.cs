using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.IntegrationTests.Firestore;

public sealed partial class FirestoreFixture
{
    [Fact]
    public async Task returns_same_typed_data_on_every_read_of_a_document_snapshot()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "snapshot-data-document");
        await document.SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("snapshot-data"));

        var snapshot = await document.GetDocumentSnapshotAsync<NullableFirestoreItem>();
        var data = FirestoreAssertions.Require(snapshot.Data);
        Assert.Same(data, snapshot.Data);

        data.NullableString = "changed";
        Assert.Equal("changed", FirestoreAssertions.Require(snapshot.Data).NullableString);

        // the cached instance belongs to one snapshot, and changing it doesn't write to Firestore
        var freshData = FirestoreAssertions.Require((await document.GetDocumentSnapshotAsync<NullableFirestoreItem>()).Data);
        Assert.NotSame(data, freshData);
        Assert.Equal("seed", freshData.NullableString);
    }

    [Fact]
    public async Task rethrows_the_same_conversion_error_on_every_read_of_a_document_snapshot()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "snapshot-data-conversion-error");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { NullableFirestoreItem.NullableNumberField, "not-a-number" }
        });

        // conversion runs when Data is first read, so getting the snapshot succeeds
        var snapshot = await document.GetDocumentSnapshotAsync<NullableFirestoreItem>();

        // the exception type is platform-specific, so only its identity is asserted
        var firstError = Record.Exception(() => snapshot.Data);
        var secondError = Record.Exception(() => snapshot.Data);
        Assert.NotNull(firstError);
        Assert.Same(firstError, secondError);
    }

    [Fact]
    public async Task returns_same_document_snapshots_on_every_enumeration_of_a_query_snapshot()
    {
        var sut = CrossFirebaseFirestore.Current;
        await GetTestingDocument(sut, "snapshot-data-query-a").SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("snapshot-data"));
        await GetTestingDocument(sut, "snapshot-data-query-b").SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("snapshot-data"));

        var snapshot = await GetTestingCollection(sut).GetDocumentsAsync<NullableFirestoreItem>();
        var firstDocuments = snapshot.Documents.ToList();
        var secondDocuments = snapshot.Documents.ToList();

        Assert.Equal(2, firstDocuments.Count);
        Assert.Equal(firstDocuments.Count, secondDocuments.Count);
        for(var i = 0; i < firstDocuments.Count; i++) {
            Assert.Same(firstDocuments[i], secondDocuments[i]);
            Assert.Same(firstDocuments[i].Data, secondDocuments[i].Data);
        }

        FirestoreAssertions.Require(firstDocuments[0].Data).NullableString = "changed";
        Assert.Equal("changed", FirestoreAssertions.Require(snapshot.Documents.First().Data).NullableString);
    }

    [Fact]
    public async Task returns_same_document_changes_on_every_read_of_a_query_snapshot()
    {
        var sut = CrossFirebaseFirestore.Current;
        await GetTestingDocument(sut, "snapshot-data-changes-a").SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("snapshot-data"));
        await GetTestingDocument(sut, "snapshot-data-changes-b").SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("snapshot-data"));

        var snapshot = await GetTestingCollection(sut).GetDocumentsAsync<NullableFirestoreItem>();
        var changes = snapshot.DocumentChanges.ToList();

        Assert.Equal(2, changes.Count);
        FirestoreAssertions.SameDocumentChanges(changes, snapshot.DocumentChanges);
        FirestoreAssertions.SameDocumentChanges(changes, snapshot.GetDocumentChanges(includeMetadataChanges: false));

        var changesWithMetadata = snapshot.GetDocumentChanges(includeMetadataChanges: true).ToList();
        Assert.Equal(2, changesWithMetadata.Count);
        FirestoreAssertions.SameDocumentChanges(changesWithMetadata, snapshot.GetDocumentChanges(includeMetadataChanges: true));
    }
}