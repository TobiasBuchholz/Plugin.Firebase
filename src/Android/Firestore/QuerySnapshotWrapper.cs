using System.Collections.Generic;
using System.Linq;
using Firebase.Firestore;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.Android.Firestore
{
    public sealed class QuerySnapshotWrapper<T> : IQuerySnapshot<T>
    {
        public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
        {
            Documents = querySnapshot.Documents.Select(x => new DocumentSnapshotWrapper<T>(x));
            Metadata = new SnapshotMetadataWrapper(querySnapshot.Metadata);
        }

        public IEnumerable<IDocumentSnapshot<T>> Documents { get; }
        public ISnapshotMetadata Metadata { get; }
    }
}