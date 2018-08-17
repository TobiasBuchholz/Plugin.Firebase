using Plugin.Firebase.Abstractions.Common;
using Plugin.Firebase.Abstractions.Firestore;
using FBFirestore = Firebase.CloudFirestore.Firestore;

namespace Plugin.Firebase.Firestore
{
    public sealed class FirebaseFirestoreImplementation : DisposableBase, IFirebaseFirestore
    {
        private readonly FBFirestore _firestore;
        
        public FirebaseFirestoreImplementation()
        {
            _firestore = FBFirestore.SharedInstance;
        }
    }
}