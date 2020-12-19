using Firebase.Firestore;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Android.Extensions;

namespace Plugin.Firebase.Android.Firestore
{
    public sealed class DocumentSnapshotWrapper<T> : DocumentSnapshotWrapper, IDocumentSnapshot<T>
    {
        public DocumentSnapshotWrapper(DocumentSnapshot documentSnapshot)
            : base(documentSnapshot)
        {
        }

        public new T Data => Wrapped.Data == null ? default(T) : Wrapped.Data.Cast<T>();
    }
    
    public class DocumentSnapshotWrapper : IDocumentSnapshot
    {
        public DocumentSnapshotWrapper(DocumentSnapshot snapshot)
        {
            Wrapped = snapshot;
        }

        public object Data => Wrapped.Data;
        public ISnapshotMetadata Metadata => Wrapped.Metadata.ToAbstract();
        public IDocumentReference Reference => Wrapped.Reference.ToAbstract();
        public DocumentSnapshot Wrapped { get; }
    }
}