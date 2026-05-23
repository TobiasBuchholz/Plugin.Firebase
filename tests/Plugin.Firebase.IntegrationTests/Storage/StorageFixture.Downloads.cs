using System.Net;
using System.Text;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage;

public sealed partial class StorageFixture
{
    [Fact]
    public async Task gets_data_as_stream()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath("files_to_keep/text_1.txt");

        var stream = await reference.GetStreamAsync(1 * 1024 * 1024);
        Assert.NotNull(stream);
    }


    [Fact]
    public async Task gets_data_as_bytes()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath("files_to_keep/text_1.txt");

        var bytes = await reference.GetBytesAsync(1 * 1024 * 1024);
        Assert.NotNull(bytes);
        Assert.Equal(34, bytes.Length);
    }


    [Fact]
    public async Task fails_when_download_exceeds_max_byte_size()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath("files_to_keep/text_1.txt");

        await Assert.ThrowsAnyAsync<Exception>(() => reference.GetBytesAsync(1));
    }


    [Fact]
    public async Task downloads_file()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath("files_to_keep/text_1.txt");

        await reference.DownloadFile($"{FileSystem.CacheDirectory}/test.txt").AwaitAsync();
    }


    [Fact]
    public async Task observes_download_success_snapshot()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath("files_to_keep/text_1.txt");
        var destinationFilePath = Path.Combine(
            FileSystem.CacheDirectory,
            IntegrationTestData.UniqueFileName("downloaded", ".txt"));
        var transferTask = reference.DownloadFile(destinationFilePath);
        var completion = new CallbackProbe<IStorageTaskSnapshot>();
        Action<IStorageTaskSnapshot> observer = snapshot => completion.TrySetResult(snapshot);
        transferTask.AddObserver(StorageTaskStatus.Success, observer);

        try {
            await transferTask.AwaitAsync();
            var snapshot = await completion.WaitAsync(
                IntegrationTestTimeouts.Callback,
                "storage download success snapshot");

            Assert.NotNull(snapshot);
            Assert.True(File.Exists(destinationFilePath));
        }
        finally {
            transferTask.RemoveObserver(observer);
        }
    }


    [Fact]
    public async Task observes_missing_download_failure_snapshot()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath($"missing/{IntegrationTestData.UniqueFileName("missing", ".txt")}");
        var destinationFilePath = Path.Combine(
            FileSystem.CacheDirectory,
            IntegrationTestData.UniqueFileName("missing", ".txt"));
        var transferTask = reference.DownloadFile(destinationFilePath);
        var failure = new CallbackProbe<IStorageTaskSnapshot>();
        Action<IStorageTaskSnapshot> observer = snapshot => failure.TrySetResult(snapshot);
        transferTask.AddObserver(StorageTaskStatus.Failure, observer);

        try {
            await Assert.ThrowsAnyAsync<Exception>(
                () => transferTask.AwaitAsync().WaitForTestAsync(
                    IntegrationTestTimeouts.LongCallback,
                    "missing storage download failure"));
            var snapshot = await failure.WaitAsync(
                IntegrationTestTimeouts.Callback,
                "storage download failure snapshot");

            Assert.NotNull(snapshot);
        }
        finally {
            transferTask.RemoveObserver(observer);
        }
    }


    [Fact]
    public void can_manage_files_download()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath("files_to_keep/text_1.txt");

        var transferTask = reference.DownloadFile($"{FileSystem.CacheDirectory}/managed.txt");
        transferTask.Pause();
        transferTask.Resume();
        transferTask.Cancel();
    }

}