using Firebase.Storage;
using Plugin.Firebase.Storage.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Storage.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firebase StorageListResult to implement IStorageListResult.
/// </summary>
public sealed class StorageListResultWrapper : IStorageListResult
{
    private readonly StorageListResult _wrapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageListResultWrapper"/> class.
    /// </summary>
    /// <param name="storageListResult">The native iOS storage list result to wrap.</param>
    public StorageListResultWrapper(StorageListResult storageListResult)
    {
        _wrapped = storageListResult;
    }

    /// <inheritdoc/>
    public IEnumerable<IStorageReference> Items => _wrapped.Items.Select(x => x.ToAbstract());

    /// <inheritdoc/>
    public IEnumerable<IStorageReference> Prefixes => _wrapped.Prefixes.Select(x => x.ToAbstract());

    /// <inheritdoc/>
    public string PageToken => _wrapped.PageToken;
}