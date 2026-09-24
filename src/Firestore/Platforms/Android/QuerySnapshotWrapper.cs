using Firebase.Firestore;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;
using NativeDocumentChange = Firebase.Firestore.DocumentChange;

namespace Plugin.Firebase.Firestore.Platforms.Android;

public sealed class QuerySnapshotWrapper<T> : IQuerySnapshot<T>
{
    private readonly QuerySnapshot _wrapped;
    private readonly SharedDocumentSnapshots<DocumentSnapshot, DocumentSnapshotWrapper<T>> _snapshots;
    // a list isn't stored if building it throws, so a native error is raised again on the next read
    private IReadOnlyList<DocumentChange<T>>? _documentChanges;
    private IReadOnlyList<DocumentChange<T>>? _documentChangesWithMetadata;

    public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
    {
        _wrapped = querySnapshot;
        _snapshots = new SharedDocumentSnapshots<DocumentSnapshot, DocumentSnapshotWrapper<T>>(
            () => querySnapshot.Documents,
            x => new DocumentSnapshotWrapper<T>(x),
            x => x.Reference.Path);
    }

    public IEnumerable<DocumentChange<T>> GetDocumentChanges(bool includeMetadataChanges)
    {
        return includeMetadataChanges
            ? LazyInitializer.EnsureInitialized(ref _documentChangesWithMetadata, () => WrapChanges(_wrapped.GetDocumentChanges(MetadataChanges.Include)))
            : LazyInitializer.EnsureInitialized(ref _documentChanges, () => WrapChanges(_wrapped.DocumentChanges));
    }

    public IEnumerable<IDocumentSnapshot<T>> Documents => _snapshots.Documents;
    public ISnapshotMetadata Metadata => _wrapped.Metadata.ToAbstract();
    public IEnumerable<DocumentChange<T>> DocumentChanges => GetDocumentChanges(false);
    public IQuery Query => _wrapped.Query.ToAbstract();
    public bool IsEmpty => _wrapped.IsEmpty;
    public int Count => _wrapped.Size();

    private IReadOnlyList<DocumentChange<T>> WrapChanges(IEnumerable<NativeDocumentChange> nativeChanges)
    {
        var changes = nativeChanges.ToList();
        var snapshots = _snapshots.GetChangedDocuments(changes.Select(x => x.Document).ToList());
        return changes.Select((x, i) => x.ToAbstract<T>(snapshots[i])).ToList().AsReadOnly();
    }
}