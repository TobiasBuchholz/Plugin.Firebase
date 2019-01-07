using System.Collections.Generic;

namespace Plugin.Firebase.Abstractions.Firestore
{
    public interface IQuerySnapshot<out T>
    {
        IEnumerable<IDocumentSnapshot<T>> Documents { get; }
        ISnapshotMetadata Metadata { get; }
    }
}