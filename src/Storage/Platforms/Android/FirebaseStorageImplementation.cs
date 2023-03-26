using Firebase.Storage;
using Plugin.Firebase.Core;
using Plugin.Firebase.Storage.Platforms.Android;

namespace Plugin.Firebase.Storage;

public sealed class FirebaseStorageImplementation : DisposableBase, IFirebaseStorage
{
    private readonly FirebaseStorage _instance;

    public FirebaseStorageImplementation()
    {
        _instance = FirebaseStorage.Instance;
    }

    public IStorageReference GetRootReference()
    {
        return _instance.Reference.ToAbstract();
    }

    public IStorageReference GetReferenceFromUrl(string url)
    {
        return _instance.GetReferenceFromUrl(url).ToAbstract();
    }

    public IStorageReference GetReferenceFromPath(string path)
    {
        return _instance.GetReference(path).ToAbstract();
    }
}