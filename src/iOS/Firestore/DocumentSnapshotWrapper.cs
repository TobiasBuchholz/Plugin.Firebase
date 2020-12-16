using Firebase.CloudFirestore;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.iOS.Extensions;

namespace Plugin.Firebase.iOS.Firestore
{
    public sealed class DocumentSnapshotWrapper<T> : DocumentSnapshotWrapper, IDocumentSnapshot<T>
    {
        public DocumentSnapshotWrapper(DocumentSnapshot documentSnapshot)
            : base(documentSnapshot)
        {
            if(documentSnapshot.Data != null) {
                Data = documentSnapshot.Data.Cast<T>();
            }
        }

        public T Data { get; }
    }
    
    public abstract class DocumentSnapshotWrapper : IDocumentSnapshot
    {
        protected DocumentSnapshotWrapper(DocumentSnapshot snapshot)
        {
            Wrapped = snapshot;
            Metadata = new SnapshotMetadataWrapper(snapshot.Metadata);
            Reference = new DocumentReferenceWrapper(snapshot.Reference);
        }

        public ISnapshotMetadata Metadata { get; }
        public IDocumentReference Reference { get; }
        public DocumentSnapshot Wrapped { get; }
    }
}