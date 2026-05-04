using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage;

internal sealed class StorageTestPathScope : IAsyncDisposable
{
    private readonly List<IStorageReference> _references = [];
    private bool _isDisposed;

    private StorageTestPathScope(IStorageReference rootReference)
    {
        RootReference = rootReference;
    }

    public IStorageReference RootReference { get; }

    public static StorageTestPathScope FromPath(string path)
    {
        return new StorageTestPathScope(CrossFirebaseStorage.Current.GetReferenceFromPath(path));
    }

    public IStorageReference Track(IStorageReference reference)
    {
        _references.Add(reference);
        return reference;
    }

    public IStorageReference Child(string path)
    {
        return Track(RootReference.GetChild(path));
    }

    public async ValueTask DisposeAsync()
    {
        if(_isDisposed) {
            return;
        }

        _isDisposed = true;
        foreach(var reference in _references.AsEnumerable().Reverse()) {
            await DeleteIfExistsAsync(reference);
        }
    }

    public static async Task DeleteChildrenIfExistsAsync(IStorageReference reference)
    {
        var children = await ListItemsIfExistsAsync(reference);
        await Task.WhenAll(children.Select(DeleteIfExistsAsync));
    }

    internal static async Task<IEnumerable<IStorageReference>> ListItemsIfExistsAsync(IStorageReference reference)
    {
        try {
            return (await reference.ListAllAsync()).Items;
        } catch(Exception e) when(e.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)) {
            TestLog.Write($"[STORAGE CLEANUP SKIP] {reference.FullPath}: {e.Message}");
            return [];
        }
    }

    internal static async Task DeleteIfExistsAsync(IStorageReference reference)
    {
        try {
            await reference.DeleteAsync();
        } catch(Exception e) {
            TestLog.Write($"[STORAGE CLEANUP ERROR] {reference.FullPath}: {e}");
        }
    }
}