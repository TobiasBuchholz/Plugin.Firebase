using Plugin.Firebase.Common;
using Plugin.Firebase.iOS.Storage;
using FirebaseStorage = Firebase.Storage.Storage;

namespace Plugin.Firebase.Storage
{
    public sealed class FirebaseStorageImplementation : DisposableBase, IFirebaseStorage
    {
        private readonly FirebaseStorage _instance;

        public FirebaseStorageImplementation()
        {
            _instance = FirebaseStorage.DefaultInstance;
        }
        
        public IStorageReference GetRootReference()
        {
            return new StorageReferenceWrapper(_instance.GetRootReference());
        }
        
        public IStorageReference GetReferenceFromUrl(string url)
        {
            return new StorageReferenceWrapper(_instance.GetReferenceFromUrl(url));
        }

        public IStorageReference GetReferenceFromPath(string path)
        {
            return new StorageReferenceWrapper(_instance.GetReferenceFromPath(path));
        }
    }
}