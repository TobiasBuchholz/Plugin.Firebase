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
    private readonly SharedDocumentSnapshots<DocumentSnapshot, DocumentSnapshotWrapper<T>> _snapshots;
    // a list isn't stored if building it throws, so a native error is raised again on the next read
    private IReadOnlyList<DocumentChange<T>>? _documentChanges;
    private IReadOnlyList<DocumentChange<T>>? _documentChangesWithMetadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySnapshotWrapper{T}"/> class.
    /// </summary>
    /// <param name="querySnapshot">The native iOS query snapshot to wrap.</param>
    public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
    {
        _wrapped = querySnapshot;
        _snapshots = new SharedDocumentSnapshots<DocumentSnapshot, DocumentSnapshotWrapper<T>>(
            () => querySnapshot.Documents,
            x => new DocumentSnapshotWrapper<T>(x),
            x => x.Reference.Path
        );
    }

    /// <inheritdoc/>
    public IEnumerable<DocumentChange<T>> GetDocumentChanges(bool includeMetadataChanges)
    {
        return includeMetadataChanges
            ? LazyInitializer.EnsureInitialized(
                ref _documentChangesWithMetadata,
                () => WrapChanges(_wrapped.GetDocumentChanges(true))
            )
            : LazyInitializer.EnsureInitialized(ref _documentChanges, () => WrapChanges(_wrapped.DocumentChanges));
    }

    /// <inheritdoc/>
    public IEnumerable<IDocumentSnapshot<T>> Documents => _snapshots.Documents;

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

    private IReadOnlyList<DocumentChange<T>> WrapChanges(IEnumerable<NativeDocumentChange> nativeChanges)
    {
        var changes = nativeChanges.ToList();
        var snapshots = _snapshots.GetChangedDocuments(changes.Select(x => x.Document).ToList());
        return changes.Select((x, i) => x.ToAbstract<T>(snapshots[i])).ToList().AsReadOnly();
    }
}