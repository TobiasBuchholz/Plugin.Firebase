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
            return _instance.GetRootReference().ToAbstract();
        }

        public IStorageReference GetReferenceFromUrl(string url)
        {
            return _instance.GetReferenceFromUrl(url).ToAbstract();
        }

        public IStorageReference GetReferenceFromPath(string path)
        {
            return _instance.GetReferenceFromPath(path).ToAbstract();
        }
    }
}