using Firebase.CloudFirestore;
using Plugin.Firebase.Abstractions.Firestore;

namespace Plugin.Firebase.Firestore
{
    public sealed class CollectionReferenceWrapper : ICollectionReference
    {
        private readonly CollectionReference _reference;
        
        public CollectionReferenceWrapper(CollectionReference reference)
        {
            _reference = reference;
        }

        public IDocumentReference GetDocument(string documentPath)
        {
            return new DocumentReferenceWrapper(_reference.GetDocument(documentPath));
        }
    }
}