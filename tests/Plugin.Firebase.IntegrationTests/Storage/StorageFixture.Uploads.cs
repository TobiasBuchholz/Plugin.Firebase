using System.Net;
using System.Text;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage;

public sealed partial class StorageFixture
{
    [Fact]
    public async Task uploads_via_byte_array()
    {
        const string path = "texts/via_bytes.txt";
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath(path);

        await reference.PutBytes("Some test text"u8.ToArray()).AwaitAsync();
        var downloadUrl = await reference.GetDownloadUrlAsync();
        StorageAssertions.DownloadUrl(path, downloadUrl);
    }


    [Fact]
    public async Task uploads_via_stream()
    {
        const string path = "texts/via_stream.txt";
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath(path);

        await using var stream = await CreateTextStreamAsync("Some text via stream");
        await reference.PutStream(stream).AwaitAsync();
        var downloadUrl = await reference.GetDownloadUrlAsync();
        StorageAssertions.DownloadUrl(path, downloadUrl);
    }


    [Fact]
    public async Task uploads_via_file_path()
    {
        const string path = "texts/via_file.txt";
        const string contents = "Some text via file";
        var filePath = await CreateTempTextFileAsync("via_file.txt", contents);
        var reference = CrossFirebaseStorage.Current.GetReferenceFromPath(path);

        await reference.PutFile(filePath).AwaitAsync();

        var bytes = await reference.GetBytesAsync(1 * 1024 * 1024);
        Assert.Equal(contents, Encoding.UTF8.GetString(bytes));
    }


    [Fact]
    public async Task uploads_stream_with_meta_data()
    {
        const string path = "texts/via_stream_with_metadata.txt";
        var metadata = new StorageMetadata(contentType: "text/plain");
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath(path);

        await reference.PutBytes("Some test text"u8.ToArray(), metadata).AwaitAsync();
        var uploadedMetadata = await reference.GetMetadataAsync();

        Assert.Equal(path, uploadedMetadata.Path);
        Assert.Equal("text/plain", uploadedMetadata.ContentType);
        Assert.Equal(14, uploadedMetadata.Size);

        var customData = new Dictionary<string, string> { { "some_key", "some_value" } };
        var updatedMetadata = await reference.UpdateMetadataAsync(new StorageMetadata(contentType: "text/html", customMetadata: customData));

        Assert.Equal(path, updatedMetadata.Path);
        Assert.Equal("text/html", updatedMetadata.ContentType);
        Assert.Equal(customData, updatedMetadata.CustomMetadata);
    }


    [Fact]
    public async Task observes_upload_success_snapshot()
    {
        const string path = "texts/upload_success_snapshot.txt";
        var reference = CrossFirebaseStorage.Current.GetReferenceFromPath(path);
        var transferTask = reference.PutBytes("Observe upload success"u8.ToArray());
        var completion = new CallbackProbe<IStorageTaskSnapshot>();
        Action<IStorageTaskSnapshot> observer = snapshot => completion.TrySetResult(snapshot);
        transferTask.AddObserver(StorageTaskStatus.Success, observer);

        try {
            await transferTask.AwaitAsync();
            var snapshot = await completion.WaitAsync(
                IntegrationTestTimeouts.Callback,
                "storage upload success snapshot");

            Assert.NotNull(snapshot);
            Assert.NotNull(snapshot.Metadata);
            Assert.True(snapshot.TransferredUnitCount > 0);
            Assert.InRange(snapshot.TransferredFraction, 0.99, 1.01);
        }
        finally {
            transferTask.RemoveObserver(observer);
        }
    }


    [Fact]
    public void can_manage_files_upload()
    {
        const string path = "texts/managed.txt";
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath(path);

        var transferTask = reference.PutBytes("Some test text"u8.ToArray());
        transferTask.Pause();
        transferTask.Resume();
        transferTask.Cancel();
    }

}