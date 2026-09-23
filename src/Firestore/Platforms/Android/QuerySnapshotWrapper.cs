using Firebase.Firestore;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;
using NativeDocumentChange = Firebase.Firestore.DocumentChange;

namespace Plugin.Firebase.Firestore.Platforms.Android;

public sealed class QuerySnapshotWrapper<T> : IQuerySnapshot<T>
{
    private readonly QuerySnapshot _wrapped;
    private readonly Lazy<IReadOnlyList<IDocumentSnapshot<T>>> _documents;
    private readonly Lazy<Dictionary<string, IDocumentSnapshot<T>>> _documentsByPath;
    private readonly Lazy<IReadOnlyList<DocumentChange<T>>> _documentChanges;
    private readonly Lazy<IReadOnlyList<DocumentChange<T>>> _documentChangesWithMetadata;

    public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
    {
        _wrapped = querySnapshot;
        _documents = CreateList(() => _wrapped.Documents.Select(x => x.ToAbstract<T>()));
        _documentsByPath = new Lazy<Dictionary<string, IDocumentSnapshot<T>>>(
            () => _documents.Value.ToDictionary(x => x.Reference.Path),
            LazyThreadSafetyMode.PublicationOnly);
        _documentChanges = CreateList(() => _wrapped.DocumentChanges.Select(WrapChange));
        _documentChangesWithMetadata = CreateList(() => _wrapped.GetDocumentChanges(MetadataChanges.Include).Select(WrapChange));
    }

    public IEnumerable<DocumentChange<T>> GetDocumentChanges(bool includeMetadataChanges)
    {
        return includeMetadataChanges ? _documentChangesWithMetadata.Value : _documentChanges.Value;
    }

    public IEnumerable<IDocumentSnapshot<T>> Documents => _documents.Value;
    public ISnapshotMetadata Metadata => _wrapped.Metadata.ToAbstract();
    public IEnumerable<DocumentChange<T>> DocumentChanges => _documentChanges.Value;
    public IQuery Query => _wrapped.Query.ToAbstract();
    public bool IsEmpty => _wrapped.IsEmpty;
    public int Count => _wrapped.Size();

    private DocumentChange<T> WrapChange(NativeDocumentChange change)
    {
        // added and modified documents are also in Documents, so they share its snapshots; removed ones get their own
        var document = _documentsByPath.Value.TryGetValue(change.Document.Reference.Path, out var current)
            ? current
            : change.Document.ToAbstract<T>();
        return new DocumentChange<T>(document, change.GetType().ToAbstract(), change.NewIndex, change.OldIndex);
    }

    // PublicationOnly doesn't store a failed build, so a native error is raised again on the next read
    private static Lazy<IReadOnlyList<TItem>> CreateList<TItem>(Func<IEnumerable<TItem>> items)
    {
        return new Lazy<IReadOnlyList<TItem>>(() => items().ToList().AsReadOnly(), LazyThreadSafetyMode.PublicationOnly);
    }
}