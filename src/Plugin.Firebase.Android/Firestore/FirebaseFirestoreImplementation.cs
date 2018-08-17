using Firebase.Firestore;
using Plugin.Firebase.Abstractions.Common;
using Plugin.Firebase.Abstractions.Firestore;
using Plugin.Firebase.Android;

namespace Plugin.Firebase.Firestore
{
    public class FirebaseFirestoreImplementation : DisposableBase, IFirebaseFirestore
    {
        private readonly FirebaseFirestore _firestore;
        
        public FirebaseFirestoreImplementation()
        {
            // FirebaseFirestore.Instance throws an exception, because the projectId of FirebaseApp.Instance is null
            // this workaround will be needed until it's fixed via https://github.com/xamarin/GooglePlayServicesComponents/commit/723ebdc00867a4c70c51ad2d0dcbd36474ce8ff1
            _firestore = FirebaseFirestore.GetInstance(CrossFirebase.Current);
        }
    }
}