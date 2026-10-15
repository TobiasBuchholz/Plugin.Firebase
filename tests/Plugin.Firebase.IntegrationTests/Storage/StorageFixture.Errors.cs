using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage;

public sealed partial class StorageFixture
{
    [Fact]
    public async Task fails_to_get_metadata_of_missing_object()
    {
        var reference = MissingObjectReference();

        var exception = await Assert.ThrowsAsync<CrossPlatformFirebaseStorageException>(
            () => reference.GetMetadataAsync());

        StorageErrorAssertions.ObjectNotFound(exception);
    }


    [Fact]
    public async Task fails_to_update_metadata_of_missing_object()
    {
        var reference = MissingObjectReference();

        var exception = await Assert.ThrowsAsync<CrossPlatformFirebaseStorageException>(
            () => reference.UpdateMetadataAsync(new StorageMetadata(contentType: "text/plain")));

        StorageErrorAssertions.ObjectNotFound(exception);
    }


    [Fact]
    public async Task fails_to_get_download_url_of_missing_object()
    {
        var reference = MissingObjectReference();

        var exception = await Assert.ThrowsAsync<CrossPlatformFirebaseStorageException>(
            () => reference.GetDownloadUrlAsync());

        StorageErrorAssertions.ObjectNotFound(exception);
    }


    [Fact]
    public async Task fails_to_delete_missing_object()
    {
        var reference = MissingObjectReference();

        var exception = await Assert.ThrowsAsync<CrossPlatformFirebaseStorageException>(
            () => reference.DeleteAsync());

        StorageErrorAssertions.ObjectNotFound(exception);
    }


    [Fact]
    public async Task fails_to_get_bytes_of_missing_object()
    {
        var reference = MissingObjectReference();

        var exception = await Assert.ThrowsAsync<CrossPlatformFirebaseStorageException>(
            () => reference.GetBytesAsync(1 * 1024 * 1024));

        StorageErrorAssertions.ObjectNotFound(exception);
    }


    [Fact]
    public async Task rejects_list_max_results_outside_native_range()
    {
        var reference = CrossFirebaseStorage
            .Current
            .GetReferenceFromPath("files_to_keep");

        // 4294967297 truncates to 1 when cast to int.
        foreach(var maxResults in new[] { 0L, -1L, 1001L, 4294967297L }) {
            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => reference.ListAsync(maxResults));
            Assert.Equal("maxResults", exception.ParamName);
        }

        var result = await reference.ListAsync(1000);
        Assert.Equal(3, result.Items.Count());
    }

    private static IStorageReference MissingObjectReference()
    {
        return CrossFirebaseStorage
            .Current
            .GetReferenceFromPath($"missing/{IntegrationTestData.UniqueFileName("missing", ".txt")}");
    }
}