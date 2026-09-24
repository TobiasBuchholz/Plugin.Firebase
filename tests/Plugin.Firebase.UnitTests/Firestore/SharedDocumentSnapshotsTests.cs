using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.UnitTests;

public class SharedDocumentSnapshotsTests
{
    private static readonly NativeDocument A = new("items/a");
    private static readonly NativeDocument B = new("items/b");
    private static readonly NativeDocument Removed = new("items/removed");

    [Fact]
    public void reading_only_documents_never_looks_up_paths()
    {
        var native = new NativeQuery(A, B);

        var documents = native.Snapshots.Documents;

        Assert.Same(documents, native.Snapshots.Documents);
        Assert.Equal(new[] { A, B }, documents.Select(x => x.Document));
        Assert.Equal(0, native.PathLookups);
        Assert.Equal(1, native.DocumentReads);
    }

    [Fact]
    public void reading_one_change_list_wraps_only_the_changed_documents_without_looking_up_paths()
    {
        var native = new NativeQuery(A, B);

        var changed = native.Snapshots.GetChangedDocuments([B]);

        Assert.Equal(new[] { B }, changed.Select(x => x.Document));
        Assert.Equal(0, native.DocumentReads);
        Assert.Equal(1, native.Wraps);
        Assert.Equal(0, native.PathLookups);
    }

    [Fact]
    public void changes_reuse_the_snapshots_of_documents_read_first()
    {
        var native = new NativeQuery(A, B);
        var documents = native.Snapshots.Documents;

        var changed = native.Snapshots.GetChangedDocuments([B, Removed]);

        Assert.Same(documents[1], changed[0]);
        Assert.Equal(Removed, changed[1].Document);
        Assert.DoesNotContain(changed[1], documents);
    }

    [Fact]
    public void documents_reuse_the_snapshots_of_changes_read_first()
    {
        var native = new NativeQuery(A, B);
        var changed = native.Snapshots.GetChangedDocuments([B]);

        var documents = native.Snapshots.Documents;

        Assert.Same(changed[0], documents[1]);
        Assert.Equal(A, documents[0].Document);
    }

    [Fact]
    public void change_lists_share_the_snapshot_of_a_removed_document()
    {
        var native = new NativeQuery(A);

        var withoutMetadata = native.Snapshots.GetChangedDocuments([Removed]);
        var withMetadata = native.Snapshots.GetChangedDocuments([A, Removed]);

        Assert.Same(withoutMetadata[0], withMetadata[1]);
    }

    [Fact]
    public void a_third_list_reuses_the_snapshots_of_the_first_two()
    {
        var native = new NativeQuery(A, B);
        var withoutMetadata = native.Snapshots.GetChangedDocuments([A, Removed]);
        var documents = native.Snapshots.Documents;

        var withMetadata = native.Snapshots.GetChangedDocuments([A, B, Removed]);

        Assert.Same(documents[0], withMetadata[0]);
        Assert.Same(documents[1], withMetadata[1]);
        Assert.Same(withoutMetadata[1], withMetadata[2]);
        Assert.Same(withoutMetadata[0], documents[0]);
    }

    [Fact]
    public void documents_built_while_a_change_list_is_created_still_share_its_snapshots()
    {
        IReadOnlyList<Snapshot>? changed = null;
        var native = new NativeQuery(A, B);
        // the change list is built after the document list is read from the native snapshot but before it's published
        native.OnDocumentsRead = () => changed = native.Snapshots.GetChangedDocuments([A]);

        var documents = native.Snapshots.Documents;

        Assert.NotNull(changed);
        Assert.Same(changed[0], documents[0]);
    }

    [Fact]
    public void a_failed_document_read_is_not_stored()
    {
        var native = new NativeQuery(A) { FailNextDocumentRead = true };

        Assert.Throws<InvalidOperationException>(() => native.Snapshots.Documents);
        var documents = native.Snapshots.Documents;

        Assert.Equal(new[] { A }, documents.Select(x => x.Document));
    }

    private sealed record NativeDocument(string Path);

    private sealed class Snapshot(NativeDocument document)
    {
        public NativeDocument Document { get; } = document;
    }

    private sealed class NativeQuery
    {
        private readonly NativeDocument[] _documents;

        public NativeQuery(params NativeDocument[] documents)
        {
            _documents = documents;
            Snapshots = new SharedDocumentSnapshots<NativeDocument, Snapshot>(
                ReadDocuments,
                x => {
                    Wraps++;
                    return new Snapshot(x);
                },
                x => {
                    PathLookups++;
                    return x.Path;
                });
        }

        public SharedDocumentSnapshots<NativeDocument, Snapshot> Snapshots { get; }
        public int DocumentReads { get; private set; }
        public int PathLookups { get; private set; }
        public int Wraps { get; private set; }
        public bool FailNextDocumentRead { get; set; }
        public Action? OnDocumentsRead { get; set; }

        private IEnumerable<NativeDocument> ReadDocuments()
        {
            DocumentReads++;
            if(FailNextDocumentRead) {
                FailNextDocumentRead = false;
                throw new InvalidOperationException("native read failed");
            }
            OnDocumentsRead?.Invoke();
            return _documents;
        }
    }
}