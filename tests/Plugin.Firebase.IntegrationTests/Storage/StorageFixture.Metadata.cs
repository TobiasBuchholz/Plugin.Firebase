using System.Net;
using System.Text;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests.Storage;

public sealed partial class StorageFixture
{
    [Fact]
    public async Task metadata_exposes_reference_and_timestamps()
    {
        const string path = "texts/metadata_properties.txt";
        var reference = CrossFirebaseStorage.Current.GetReferenceFromPath(path);

        var metadataToUpload = new StorageMetadata(
            cacheControl: "public,max-age=60",
            contentDisposition: "inline",
            contentEncoding: "identity",
            contentLanguage: "en",
            contentType: "text/plain");

        await reference.PutBytes("metadata properties"u8.ToArray(), metadataToUpload).AwaitAsync();
        var metadata = await reference.GetMetadataAsync();

        Assert.Equal(StorageAssertions.ExpectedBucket(), metadata.Bucket);
        Assert.Equal("metadata_properties.txt", metadata.Name);
        Assert.Equal(path, metadata.Path);
        Assert.Equal("public,max-age=60", metadata.CacheControl);
        Assert.Equal("inline", metadata.ContentDisposition);
        Assert.Equal("identity", metadata.ContentEncoding);
        Assert.Equal("en", metadata.ContentLanguage);
        Assert.Equal("text/plain", metadata.ContentType);
        Assert.True(metadata.Generation.HasValue);
        Assert.True(metadata.MetaGeneration.HasValue);
        if(OperatingSystem.IsAndroid()) {
            var storageReference = metadata.StorageReference;
            Assert.NotNull(storageReference);
            Assert.Equal(reference.FullPath, storageReference.FullPath);
        } else {
            Assert.Null(metadata.StorageReference);
        }
        Assert.True(metadata.CreationTime.HasValue);
        Assert.True(metadata.UpdatedTime.HasValue);
        Assert.NotEqual(default, metadata.CreationTime.Value);
        Assert.NotEqual(default, metadata.UpdatedTime.Value);
    }

}
