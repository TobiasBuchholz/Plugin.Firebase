using Firebase.CloudFirestore;
using Plugin.Firebase.Firestore.Platforms.iOS.Extensions;
using NativeDocumentChange = Firebase.CloudFirestore.DocumentChange;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firestore query snapshot with typed document data.
/// </summary>
/// <typeparam name="T">The type to deserialize document data into.</typeparam>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySnapshotWrapper{T}"/> class.
    /// </summary>
    /// <param name="querySnapshot">The native iOS query snapshot to wrap.</param>
    public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
    {
        _wrapped = querySnapshot;
    }

    /// <inheritdoc/>
    public IEnumerable<DocumentChange<T>> GetDocumentChanges(bool includeMetadataChanges)
    {
        lock(_lock) {
            return includeMetadataChanges
                ? _documentChangesWithMetadata ??= WrapChanges(_wrapped.GetDocumentChanges(true))
                : _documentChanges ??= WrapChanges(_wrapped.DocumentChanges);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IDocumentSnapshot<T>> Documents {
        get {
            lock(_lock) {
                return _documents ??= _wrapped.Documents.Select(GetSnapshot).ToList().AsReadOnly();
            }
        }
    }

    /// <inheritdoc/>
    public ISnapshotMetadata Metadata => _wrapped.Metadata.ToAbstract();

    /// <inheritdoc/>
    public IEnumerable<DocumentChange<T>> DocumentChanges => GetDocumentChanges(false);

    /// <inheritdoc/>
    public IQuery Query => _wrapped.Query.ToAbstract();

    /// <inheritdoc/>
    public bool IsEmpty => _wrapped.IsEmpty;

    /// <inheritdoc/>
    public int Count => (int) _wrapped.Count;

    private IReadOnlyList<DocumentChange<T>> WrapChanges(IEnumerable<NativeDocumentChange> changes)
    {
        // index Documents if it's already built, so added and modified documents reuse its snapshots
        _snapshotsByPath ??=
            _documents?.ToDictionary(x => x.Wrapped.Reference.Path)
            ?? new Dictionary<string, DocumentSnapshotWrapper<T>>();
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