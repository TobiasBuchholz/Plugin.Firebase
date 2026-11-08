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


    [AndroidFact]
    public void rejects_list_max_results_that_overflow_native_integer()
    {
        var reference = CrossFirebaseStorage.Current.GetReferenceFromPath("files_to_keep");

        // Both values would become 1 after an unchecked cast to int.
        foreach(var maxResults in new[] { 4294967297L, -4294967295L }) {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => {
                _ = reference.ListAsync(maxResults);
            });
            Assert.Equal("maxResults", exception.ParamName);
        }
    }

    private static IStorageReference MissingObjectReference()
    {
        return CrossFirebaseStorage
            .Current
            .GetReferenceFromPath($"missing/{IntegrationTestData.UniqueFileName("missing", ".txt")}");
    }
}