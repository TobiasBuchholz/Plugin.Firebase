using System.Text;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestFixture(IntegrationTestPackage.Storage)]
[Preserve(AllMembers = true)]
public sealed partial class StorageFixture : IAsyncLifetime
{
    private static readonly SemaphoreSlim SeedLock = new(1, 1);
    private static bool _storageEmulatorSeeded;
    private static readonly string[] Expected = ["/prefix_listing/folder_a", "/prefix_listing/folder_b"];

    public async Task InitializeAsync()
    {
        await EnsureStorageEmulatorSeedDataAsync();
    }

    private static bool UsesStorageEmulator()
    {
        return IntegrationTestEnvironment.ShouldUseStorageEmulator;
    }

    private static async Task<Stream> CreateTextStreamAsync(string text)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(text);
        await writer.FlushAsync();
        return stream;
    }

    private static async Task<string> CreateTempTextFileAsync(string fileName, string text)
    {
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllTextAsync(filePath, text);
        return filePath;
    }

    public async Task DisposeAsync()
    {
        TestLog.Write("[STORAGE CLEANUP START]");
        var rootReference = CrossFirebaseStorage.Current.GetRootReference();
        await StorageTestPathScope.DeleteChildrenIfExistsAsync(rootReference.GetChild("files_to_delete"));
        await StorageTestPathScope.DeleteChildrenIfExistsAsync(rootReference.GetChild("texts"));
        TestLog.Write("[STORAGE CLEANUP END]");
    }

    private static async Task EnsureStorageEmulatorSeedDataAsync()
    {
        if(!UsesStorageEmulator() || _storageEmulatorSeeded) {
            return;
        }

        await SeedLock.WaitAsync();
        try {
            if(_storageEmulatorSeeded) {
                return;
            }

            var rootReference = CrossFirebaseStorage.Current.GetRootReference();
            var filesToKeep = rootReference.GetChild("files_to_keep");
            var existingItems = await StorageTestPathScope.ListItemsIfExistsAsync(filesToKeep);
            await Task.WhenAll(existingItems.Select(x => x.DeleteAsync()));

            await EnsureSeedFileAsync(
                filesToKeep,
                "text_1.txt",
                "0123456789012345678901234567890123"u8.ToArray());
            await EnsureSeedFileAsync(
                filesToKeep,
                "text_2.txt",
                "text-file-two"u8.ToArray());
            await EnsureSeedFileAsync(
                filesToKeep,
                "text_3.txt",
                "text-file-three"u8.ToArray());

            _storageEmulatorSeeded = true;
        }
        finally {
            SeedLock.Release();
        }
    }

    private static async Task EnsureSeedFileAsync(
        IStorageReference parentReference,
        string fileName,
        byte[] contents)
    {
        await parentReference.GetChild(fileName).PutBytes(contents).AwaitAsync();
    }
}