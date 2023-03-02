using Firebase.Storage;

namespace Plugin.Firebase.Storage.Platforms.Android;

public sealed class StorageListResultWrapper : IStorageListResult
{
    private readonly ListResult _wrapped;

    public StorageListResultWrapper(ListResult wrapped)
    {
        _wrapped = wrapped;
    }

    public IEnumerable<IStorageReference> Items => _wrapped.Items.Select(x => x.ToAbstract());
    public IEnumerable<IStorageReference> Prefixes => _wrapped.Prefixes.Select(x => x.ToAbstract());
    public string PageToken => _wrapped.PageToken;
}