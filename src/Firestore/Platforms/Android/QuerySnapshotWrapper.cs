using Firebase.Firestore;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Firestore.Android.Extensions;

namespace Plugin.Firebase.Android.Firestore;

public sealed class QuerySnapshotWrapper<T> : IQuerySnapshot<T>
{
    private readonly QuerySnapshot _wrapped;

    public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
    {
        _wrapped = querySnapshot;
    }

    public IEnumerable<DocumentChange<T>> GetDocumentChanges(bool includeMetadataChanges)
    {
        return _wrapped
            .GetDocumentChanges(includeMetadataChanges ? MetadataChanges.Include : MetadataChanges.Exclude)
            .Select(x => x.ToAbstract<T>());
    }

    public IEnumerable<IDocumentSnapshot<T>> Documents => _wrapped.Documents.Select(x => x.ToAbstract<T>());
    public ISnapshotMetadata Metadata => _wrapped.Metadata.ToAbstract();
    public IEnumerable<DocumentChange<T>> DocumentChanges => _wrapped.DocumentChanges.Select(x => x.ToAbstract<T>());
    public IQuery Query => _wrapped.Query.ToAbstract();
    public bool IsEmpty => _wrapped.IsEmpty;
    public int Count => _wrapped.Size();
}