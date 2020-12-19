using System.Collections.Generic;
using System.Linq;
using Firebase.CloudFirestore;
using Plugin.Firebase.Firestore;
using DocumentChange = Plugin.Firebase.Firestore.DocumentChange;

namespace Plugin.Firebase.iOS.Firestore
{
    public sealed class QuerySnapshotWrapper<T> : IQuerySnapshot<T>
    {
        private readonly QuerySnapshot _wrapped;
        
        public QuerySnapshotWrapper(QuerySnapshot querySnapshot)
        {
            _wrapped = querySnapshot;
        }

        public IEnumerable<DocumentChange> GetDocumentChanges(bool includeMetadataChanges)
        {
            return _wrapped
                .GetDocumentChanges(includeMetadataChanges)
                .Select(x => x.ToAbstract());
        }

        public IEnumerable<IDocumentSnapshot<T>> Documents => _wrapped.Documents.Select(x => x.ToAbstract<T>());
        public ISnapshotMetadata Metadata => _wrapped.Metadata.ToAbstract();
        public IEnumerable<DocumentChange> DocumentChanges => _wrapped.DocumentChanges.Select(x => x.ToAbstract());
        public IQuery Query => _wrapped.Query.ToAbstract();
        public bool IsEmpty => _wrapped.IsEmpty;
        public int Count => (int) _wrapped.Count;
    }
}