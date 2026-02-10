using Plugin.Firebase.Core;
using Plugin.Firebase.Storage.Platforms.iOS.Extensions;
using FirebaseStorage = Firebase.Storage.Storage;

namespace Plugin.Firebase.Storage;

/// <summary>
/// iOS implementation of Firebase Storage that wraps the native iOS Firebase Storage SDK.
/// </summary>
public sealed class FirebaseStorageImplementation : DisposableBase, IFirebaseStorage
{
    private readonly FirebaseStorage _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirebaseStorageImplementation"/> class.
    /// </summary>
    public FirebaseStorageImplementation()
    {
        _instance = FirebaseStorage.DefaultInstance;
    }

    /// <inheritdoc/>
    public IStorageReference GetRootReference()
    {
        return _instance.GetRootReference().ToAbstract();
    }

    /// <inheritdoc/>
    public IStorageReference GetReferenceFromUrl(string url)
    {
        return _instance.GetReferenceFromUrl(url).ToAbstract();
    }

    /// <inheritdoc/>
    public IStorageReference GetReferenceFromPath(string path)
    {
        return _instance.GetReferenceFromPath(path).ToAbstract();
    }
}