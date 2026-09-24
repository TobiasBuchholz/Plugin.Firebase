using Firebase.Firestore;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;
using NativeDocumentChange = Firebase.Firestore.DocumentChange;

namespace Plugin.Firebase.Firestore.Platforms.Android;

public sealed class QuerySnapshotWrapper<T> : IQuerySnapshot<T>
{
    private readonly QuerySnapshot _wrapped;
    // guards the lists below; building them only creates wrappers, so no model code runs while it's held. A list
    // stays null if building it throws, so a native error is raised again on the next read
    private readonly object _lock = new();
    private IReadOnlyList<DocumentSnapshotWrapper<T>>? _documents;
    private IReadOnlyList<DocumentChange<T>>? _documentChanges;
    private IReadOnlyList<DocumentChange<T>>? _documentChangesWithMetadata;
    // created with the first change list, so reading only Documents never looks up document paths
    private Dictionary<string, DocumentSnapshotWrapper<T>>? _snapshotsByPath;

    public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
    {
        _wrapped = querySnapshot;
    }

    public IEnumerable<DocumentChange<T>> GetDocumentChanges(bool includeMetadataChanges)
    {
        lock(_lock) {
            return includeMetadataChanges
                ? _documentChangesWithMetadata ??= WrapChanges(_wrapped.GetDocumentChanges(MetadataChanges.Include))
                : _documentChanges ??= WrapChanges(_wrapped.DocumentChanges);
        }
    }

    public IEnumerable<IDocumentSnapshot<T>> Documents {
        get {
            lock(_lock) {
                return _documents ??= _wrapped.Documents.Select(GetSnapshot).ToList().AsReadOnly();
            }
        }
    }

    public ISnapshotMetadata Metadata => _wrapped.Metadata.ToAbstract();
    public IEnumerable<DocumentChange<T>> DocumentChanges => GetDocumentChanges(false);
    public IQuery Query => _wrapped.Query.ToAbstract();
    public bool IsEmpty => _wrapped.IsEmpty;
    public int Count => _wrapped.Size();

    private IReadOnlyList<DocumentChange<T>> WrapChanges(IEnumerable<NativeDocumentChange> changes)
    {
        // index Documents if it's already built, so added and modified documents reuse its snapshots
        _snapshotsByPath ??= _documents?.ToDictionary(x => x.Wrapped.Reference.Path) ?? new Dictionary<string, DocumentSnapshotWrapper<T>>();
        return changes.Select(x => x.ToAbstract<T>(GetSnapshot(x.Document))).ToList().AsReadOnly();
    }

    private DocumentSnapshotWrapper<T> GetSnapshot(DocumentSnapshot document)
    {
        if(_snapshotsByPath == null) {
            return new DocumentSnapshotWrapper<T>(document);
        }

        var path = document.Reference.Path;
        if(!_snapshotsByPath.TryGetValue(path, out var snapshot)) {
            snapshot = new DocumentSnapshotWrapper<T>(document);
            _snapshotsByPath.Add(path, snapshot);
        }
        return snapshot;
    }
}