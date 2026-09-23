using Firebase.Firestore;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;

namespace Plugin.Firebase.Firestore.Platforms.Android;

public sealed class QuerySnapshotWrapper<T> : IQuerySnapshot<T>
{
    private readonly QuerySnapshot _wrapped;
    private readonly Lazy<IReadOnlyList<IDocumentSnapshot<T>>> _documents;
    private readonly Lazy<IReadOnlyList<DocumentChange<T>>> _documentChanges;
    private readonly Lazy<IReadOnlyList<DocumentChange<T>>> _documentChangesWithMetadata;

    public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
    {
        _wrapped = querySnapshot;
        _documents = new Lazy<IReadOnlyList<IDocumentSnapshot<T>>>(
            () => _wrapped.Documents.Select(x => x.ToAbstract<T>()).ToList().AsReadOnly());
        _documentChanges = new Lazy<IReadOnlyList<DocumentChange<T>>>(
            () => _wrapped.DocumentChanges.Select(x => x.ToAbstract<T>()).ToList().AsReadOnly());
        _documentChangesWithMetadata = new Lazy<IReadOnlyList<DocumentChange<T>>>(
            () => _wrapped.GetDocumentChanges(MetadataChanges.Include).Select(x => x.ToAbstract<T>()).ToList().AsReadOnly());
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
}