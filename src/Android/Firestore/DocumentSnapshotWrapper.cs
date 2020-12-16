using Firebase.Firestore;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Android.Extensions;

namespace Plugin.Firebase.Android.Firestore
{
    public sealed class DocumentSnapshotWrapper<T> : DocumentSnapshotWrapper, IDocumentSnapshot<T>
    {
        public DocumentSnapshotWrapper(DocumentSnapshot snapshot)
            : base(snapshot)
        {
            if(snapshot.Data != null) {
                Data = snapshot.Data.Cast<T>();
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