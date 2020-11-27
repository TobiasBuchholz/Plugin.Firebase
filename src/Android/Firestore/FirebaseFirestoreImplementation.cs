using Firebase.Firestore;
using Plugin.Firebase.Common;
using Plugin.Firebase.Android.Firestore;

namespace Plugin.Firebase.Firestore
{
    public class FirebaseFirestoreImplementation : DisposableBase, IFirebaseFirestore
    {
        private readonly FirebaseFirestore _firestore;
        
        public FirebaseFirestoreImplementation()
        {
            _firestore = FirebaseFirestore.Instance;
        }

        public ICollectionReference GetCollection(string collectionPath)
        {
            return new CollectionReferenceWrapper(_firestore.Collection(collectionPath));
        }

        public IDocumentReference GetDocument(string documentPath)
        {
            return new DocumentReferenceWrapper(_firestore.Document(documentPath));
        }
    }
}