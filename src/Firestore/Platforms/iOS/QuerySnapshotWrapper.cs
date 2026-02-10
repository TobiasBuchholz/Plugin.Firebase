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
        return _wrapped.GetDocumentChanges(includeMetadataChanges).Select(x => x.ToAbstract<T>());
    }

    /// <inheritdoc/>
    public IEnumerable<IDocumentSnapshot<T>> Documents =>
        _wrapped.Documents.Select(x => x.ToAbstract<T>());

    /// <inheritdoc/>
    public ISnapshotMetadata Metadata => _wrapped.Metadata.ToAbstract();

    /// <inheritdoc/>
    public IEnumerable<DocumentChange<T>> DocumentChanges =>
        _wrapped.DocumentChanges.Select(x => x.ToAbstract<T>());

    /// <inheritdoc/>
    public IQuery Query => _wrapped.Query.ToAbstract();

    /// <inheritdoc/>
    public bool IsEmpty => _wrapped.IsEmpty;

    /// <inheritdoc/>
    public int Count => (int) _wrapped.Count;
}