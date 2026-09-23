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
    public async Task converts_again_after_a_failed_read_of_a_document_snapshot()
    {
        var sut = CrossFirebaseFirestore.Current;
        var document = GetTestingDocument(sut, "snapshot-data-conversion-error");
        await document.SetDataAsync(new Dictionary<object, object?> {
            { NullableFirestoreItem.NullableNumberField, "not-a-number" }
        });

        // conversion runs when Data is first read, so getting the snapshot succeeds
        var snapshot = await document.GetDocumentSnapshotAsync<NullableFirestoreItem>();

        // a failed conversion isn't kept, so the second read converts again and fails with a new exception;
        // Android rejects the text in Convert.ChangeType and iOS in PropertyInfo.SetValue
        var firstError = Record.Exception(() => snapshot.Data);
        var secondError = Record.Exception(() => snapshot.Data);
        Assert.True(firstError is FormatException or ArgumentException, $"Unexpected conversion error: {firstError}");
        Assert.True(secondError is FormatException or ArgumentException, $"Unexpected conversion error: {secondError}");
        Assert.NotSame(firstError, secondError);
    }

    [Fact]
    public async Task returns_same_document_snapshots_on_every_enumeration_of_a_query_snapshot()
    {
        var sut = CrossFirebaseFirestore.Current;
        await GetTestingDocument(sut, "snapshot-data-query-a").SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("snapshot-data"));
        await GetTestingDocument(sut, "snapshot-data-query-b").SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("snapshot-data"));

        var snapshot = await GetTestingCollection(sut).GetDocumentsAsync<NullableFirestoreItem>();
        var documents = snapshot.Documents.ToList();

        Assert.Equal(2, documents.Count);
        FirestoreAssertions.SameDocuments(documents, snapshot.Documents);

        FirestoreAssertions.Require(documents[0].Data).NullableString = "changed";
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

        // added documents are also in Documents, so their changes carry the same snapshots
        var documents = snapshot.Documents.ToList();
        foreach(var change in changes.Concat(changesWithMetadata)) {
            Assert.Same(documents.Single(x => x.Reference.Path == change.DocumentSnapshot.Reference.Path), change.DocumentSnapshot);
        }
    }

    [Fact]
    public async Task gives_removed_documents_their_own_snapshot_in_a_query_listener()
    {
        var sut = CrossFirebaseFirestore.Current;
        await GetTestingDocument(sut, "snapshot-data-listener-kept").SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("snapshot-data"));
        await GetTestingDocument(sut, "snapshot-data-listener-removed").SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("snapshot-data"));

        var initialSnapshot = new CallbackProbe<bool>();
        var removalSnapshot = new CallbackProbe<IQuerySnapshot<NullableFirestoreItem>>();
        using var listener = GetTestingCollection(sut).AddSnapshotListener<NullableFirestoreItem>(x => {
            if(x.Count == 2) {
                initialSnapshot.TrySetResult(true);
            }
            if(x.DocumentChanges.Any(y => y.ChangeType == DocumentChangeType.Removed)) {
                removalSnapshot.TrySetResult(x);
            }
        });
        await initialSnapshot.WaitAsync(IntegrationTestTimeouts.Callback, "initial Firestore query listener snapshot");

        await GetTestingDocument(sut, "snapshot-data-listener-removed").DeleteDocumentAsync();
        var snapshot = await removalSnapshot.WaitAsync(IntegrationTestTimeouts.Callback, "Firestore query listener removal");

        var removal = Assert.Single(snapshot.DocumentChanges);
        Assert.Equal(DocumentChangeType.Removed, removal.ChangeType);
        Assert.Equal("snapshot-data-listener-removed", FirestoreAssertions.Require(removal.DocumentSnapshot.Data).Id);
        var remaining = Assert.Single(snapshot.Documents);
        Assert.Equal("snapshot-data-listener-kept", FirestoreAssertions.Require(remaining.Data).Id);
        Assert.NotSame(remaining, removal.DocumentSnapshot);
    }

    [Fact]
    public async Task rejects_metadata_document_changes_on_every_call_for_a_listener_without_metadata_changes()
    {
        var sut = CrossFirebaseFirestore.Current;
        await GetTestingDocument(sut, "snapshot-data-listener").SetDataAsync(NullableFirestoreItemFactory.CreateNonNullItem("snapshot-data"));

        var received = new CallbackProbe<IQuerySnapshot<NullableFirestoreItem>>();
        using var listener = GetTestingCollection(sut).AddSnapshotListener<NullableFirestoreItem>(
            x => received.TrySetResult(x),
            includeMetaDataChanges: false);
        var snapshot = await received.WaitAsync(IntegrationTestTimeouts.Callback, "Firestore query listener snapshot");

        // the native SDKs reject this for a listener registered without metadata changes, and a failed read isn't
        // kept, so every call asks the native SDK again
        var firstError = Record.Exception(() => snapshot.GetDocumentChanges(includeMetadataChanges: true));
        var secondError = Record.Exception(() => snapshot.GetDocumentChanges(includeMetadataChanges: true));
        Assert.NotNull(firstError);
        Assert.NotNull(secondError);
        Assert.NotSame(firstError, secondError);
    }
}