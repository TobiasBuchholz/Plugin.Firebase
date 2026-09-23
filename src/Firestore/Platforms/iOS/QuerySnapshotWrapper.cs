using Firebase.CloudFirestore;
using Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firestore query snapshot with typed document data.
/// </summary>
/// <typeparam name="T">The type to deserialize document data into.</typeparam>
public sealed class QuerySnapshotWrapper<T> : IQuerySnapshot<T>
{
    private readonly QuerySnapshot _wrapped;
    private readonly Lazy<IReadOnlyList<IDocumentSnapshot<T>>> _documents;
    private readonly Lazy<IReadOnlyList<DocumentChange<T>>> _documentChanges;
    private readonly Lazy<IReadOnlyList<DocumentChange<T>>> _documentChangesWithMetadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySnapshotWrapper{T}"/> class.
    /// </summary>
    /// <param name="querySnapshot">The native iOS query snapshot to wrap.</param>
    public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
    {
        _wrapped = querySnapshot;
        _documents = new Lazy<IReadOnlyList<IDocumentSnapshot<T>>>(
            () => _wrapped.Documents.Select(x => x.ToAbstract<T>()).ToList().AsReadOnly()
        );
        _documentChanges = new Lazy<IReadOnlyList<DocumentChange<T>>>(
            () => _wrapped.DocumentChanges.Select(x => x.ToAbstract<T>()).ToList().AsReadOnly()
        );
        _documentChangesWithMetadata = new Lazy<IReadOnlyList<DocumentChange<T>>>(
            () => _wrapped.GetDocumentChanges(true).Select(x => x.ToAbstract<T>()).ToList().AsReadOnly()
        );
    }

    /// <inheritdoc/>
    public IEnumerable<DocumentChange<T>> GetDocumentChanges(bool includeMetadataChanges)
    {
        return includeMetadataChanges ? _documentChangesWithMetadata.Value : _documentChanges.Value;
    }

    /// <inheritdoc/>
    public IEnumerable<IDocumentSnapshot<T>> Documents => _documents.Value;

    /// <inheritdoc/>
    public ISnapshotMetadata Metadata => _wrapped.Metadata.ToAbstract();

    /// <inheritdoc/>
    public IEnumerable<DocumentChange<T>> DocumentChanges => _documentChanges.Value;

    /// <inheritdoc/>
    public IQuery Query => _wrapped.Query.ToAbstract();

    /// <inheritdoc/>
    public bool IsEmpty => _wrapped.IsEmpty;

    /// <inheritdoc/>
    public int Count => (int) _wrapped.Count;
}