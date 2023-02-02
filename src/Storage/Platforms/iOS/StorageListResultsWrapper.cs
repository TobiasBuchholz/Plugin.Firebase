using Firebase.Storage;
using Plugin.Firebase.Storage;
using Plugin.Firebase.iOS.Storage.Extensions;

namespace Plugin.Firebase.iOS.Storage;

public sealed class StorageListResultWrapper : IStorageListResult
{
    private readonly StorageListResult _wrapped;

    public StorageListResultWrapper(StorageListResult storageListResult)
    {
        _wrapped = storageListResult;
    }

    public IEnumerable<IStorageReference> Items => _wrapped.Items.Select(x => x.ToAbstract());
    public IEnumerable<IStorageReference> Prefixes => _wrapped.Prefixes.Select(x => x.ToAbstract());
    public string PageToken => _wrapped.PageToken;
}