using Firebase.CloudFirestore;
using Plugin.Firebase.Abstractions.Firestore;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.iOS.Extensions;

namespace Plugin.Firebase.iOS.Firestore
{
    public sealed class DocumentSnapshotWrapper<T> : IDocumentSnapshot<T>
    {
        public DocumentSnapshotWrapper(DocumentSnapshot documentSnapshot)
        {
            if(documentSnapshot.Data != null) {
                Data = documentSnapshot.Data.Cast<T>();
            }
            Metadata = new SnapshotMetadataWrapper(documentSnapshot.Metadata);
            Reference = new DocumentReferenceWrapper(documentSnapshot.Reference);
        }

        public T Data { get; }
        public ISnapshotMetadata Metadata { get; }
        public IDocumentReference Reference { get; }
    }
}