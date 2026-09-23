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
    private readonly Lazy<IReadOnlyList<IDocumentSnapshot<T>>> _documents;
    private readonly Lazy<Dictionary<string, IDocumentSnapshot<T>>> _documentsByPath;
    private readonly Lazy<IReadOnlyList<DocumentChange<T>>> _documentChanges;
    private readonly Lazy<IReadOnlyList<DocumentChange<T>>> _documentChangesWithMetadata;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySnapshotWrapper{T}"/> class.
    /// </summary>
    /// <param name="querySnapshot">The native iOS query snapshot to wrap.</param>
    public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
    {
        _wrapped = querySnapshot;
        _documents = CreateList(() => _wrapped.Documents.Select(x => x.ToAbstract<T>()));
        _documentsByPath = new Lazy<Dictionary<string, IDocumentSnapshot<T>>>(
            () => _documents.Value.ToDictionary(x => x.Reference.Path),
            LazyThreadSafetyMode.PublicationOnly
        );
        _documentChanges = CreateList(() => _wrapped.DocumentChanges.Select(WrapChange));
        _documentChangesWithMetadata = CreateList(() => _wrapped.GetDocumentChanges(true).Select(WrapChange));
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

    private DocumentChange<T> WrapChange(NativeDocumentChange change)
    {
        // added and modified documents are also in Documents, so they share its snapshots; removed ones get their own
        var document = _documentsByPath.Value.TryGetValue(change.Document.Reference.Path, out var current)
            ? current
            : change.Document.ToAbstract<T>();
        return new DocumentChange<T>(
            document,
            change.Type.ToAbstract(),
            (int) change.NewIndex,
            (int) change.OldIndex
        );
    }

    // PublicationOnly doesn't store a failed build, so a native error is raised again on the next read
    private static Lazy<IReadOnlyList<TItem>> CreateList<TItem>(Func<IEnumerable<TItem>> items)
    {
        return new Lazy<IReadOnlyList<TItem>>(
            () => items().ToList().AsReadOnly(),
            LazyThreadSafetyMode.PublicationOnly
        );
    }
}