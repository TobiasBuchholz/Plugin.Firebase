using System.Collections.Generic;

namespace Plugin.Firebase.Firestore
{
    public interface IQuerySnapshot<out T>
    {
        IEnumerable<DocumentChange> GetDocumentChanges(bool includeMetadataChanges);

        IEnumerable<IDocumentSnapshot<T>> Documents { get; }
        ISnapshotMetadata Metadata { get; }
        IEnumerable<DocumentChange> DocumentChanges { get; }
        IQuery Query { get; }
        bool IsEmpty { get; }
        int Count { get; }
    }
}