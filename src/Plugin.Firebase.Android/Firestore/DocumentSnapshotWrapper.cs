using Firebase.Firestore;
using Plugin.Firebase.Abstractions.Firestore;
using Plugin.Firebase.Android.Extensions;
using Plugin.Firebase.Firestore;

namespace Plugin.Firebase.Android.Firestore
{
    public sealed class DocumentSnapshotWrapper<T> : IDocumentSnapshot<T>
    {
        public DocumentSnapshotWrapper(DocumentSnapshot snapshot)
        {
            Data = snapshot.Data.Cast<T>();
            Metadata = new SnapshotMetadataWrapper(snapshot.Metadata);
            Reference = new DocumentReferenceWrapper(snapshot.Reference);
        }

        public T Data { get; }
        public ISnapshotMetadata Metadata { get; }
        public IDocumentReference Reference { get; }
    }
}