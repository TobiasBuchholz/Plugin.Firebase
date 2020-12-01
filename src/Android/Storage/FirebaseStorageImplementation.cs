using Firebase.Storage;
using Plugin.Firebase.Android.Storage;
using Plugin.Firebase.Common;

namespace Plugin.Firebase.Storage
{
    public sealed class FirebaseStorageImplementation : DisposableBase, IFirebaseStorage
    {
        private readonly FirebaseStorage _instance;

        public FirebaseStorageImplementation()
        {
            _instance = FirebaseStorage.Instance;
        }
        
        public IStorageReference GetRootReference()
        {
            return new StorageReferenceWrapper(_instance.Reference);
        }
        
        public IStorageReference GetReferenceFromUrl(string url)
        {
            return new StorageReferenceWrapper(_instance.GetReferenceFromUrl(url));
        }

        public IStorageReference GetReferenceFromPath(string path)
        {
            return new StorageReferenceWrapper(_instance.GetReference(path));
        }
    }
}