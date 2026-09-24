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
    // each list is built on its first read; it isn't stored if building it throws, so a native error is raised again on
    // the next read. The fields are checked first so that reading a built list doesn't allocate a delegate
    private IReadOnlyList<IDocumentSnapshot<T>>? _documents;
    private IReadOnlyList<DocumentChange<T>>? _documentChanges;
    private IReadOnlyList<DocumentChange<T>>? _documentChangesWithMetadata;

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
        return includeMetadataChanges
            ? _documentChangesWithMetadata
                ?? LazyInitializer.EnsureInitialized(
                    ref _documentChangesWithMetadata,
                    () => WrapChanges(_wrapped.GetDocumentChanges(true))
                )
            : _documentChanges
                ?? LazyInitializer.EnsureInitialized(ref _documentChanges, () => WrapChanges(_wrapped.DocumentChanges));
    }

    /// <inheritdoc/>
    public IEnumerable<IDocumentSnapshot<T>> Documents =>
        _documents
        ?? LazyInitializer.EnsureInitialized(
            ref _documents,
            () => _wrapped.Documents.Select(x => x.ToAbstract<T>()).ToList().AsReadOnly()
        );

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

    private static IReadOnlyList<DocumentChange<T>> WrapChanges(IEnumerable<NativeDocumentChange> changes)
    {
        return changes.Select(x => x.ToAbstract<T>()).ToList().AsReadOnly();
    }
}