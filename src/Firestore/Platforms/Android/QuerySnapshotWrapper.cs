using Firebase.Firestore;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;
using NativeDocumentChange = Firebase.Firestore.DocumentChange;

namespace Plugin.Firebase.Firestore.Platforms.Android;

public sealed class QuerySnapshotWrapper<T> : IQuerySnapshot<T>
{
    private readonly QuerySnapshot _wrapped;
    // each list is built on its first read; it isn't stored if building it throws, so a native error is raised again on
    // the next read. The fields are checked first so that reading a built list doesn't allocate a delegate
    private IReadOnlyList<IDocumentSnapshot<T>>? _documents;
    private IReadOnlyList<DocumentChange<T>>? _documentChanges;
    private IReadOnlyList<DocumentChange<T>>? _documentChangesWithMetadata;

    public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
    {
        _wrapped = querySnapshot;
    }

    public IEnumerable<DocumentChange<T>> GetDocumentChanges(bool includeMetadataChanges)
    {
        return includeMetadataChanges
            ? _documentChangesWithMetadata ?? LazyInitializer.EnsureInitialized(ref _documentChangesWithMetadata, () => WrapChanges(_wrapped.GetDocumentChanges(MetadataChanges.Include)))
            : _documentChanges ?? LazyInitializer.EnsureInitialized(ref _documentChanges, () => WrapChanges(_wrapped.DocumentChanges));
    }

    public IEnumerable<IDocumentSnapshot<T>> Documents =>
        _documents ?? LazyInitializer.EnsureInitialized(ref _documents, () => _wrapped.Documents.Select(x => x.ToAbstract<T>()).ToList().AsReadOnly());

    public ISnapshotMetadata Metadata => _wrapped.Metadata.ToAbstract();
    public IEnumerable<DocumentChange<T>> DocumentChanges => GetDocumentChanges(false);
    public IQuery Query => _wrapped.Query.ToAbstract();
    public bool IsEmpty => _wrapped.IsEmpty;
    public int Count => _wrapped.Size();

    private static IReadOnlyList<DocumentChange<T>> WrapChanges(IEnumerable<NativeDocumentChange> changes)
    {
        return changes.Select(x => x.ToAbstract<T>()).ToList().AsReadOnly();
    }
}