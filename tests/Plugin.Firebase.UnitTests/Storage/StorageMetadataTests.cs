using NSubstitute;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.UnitTests;

public class StorageMetadataTests
{
    [Fact]
    public void constructor_defaults_optional_values_to_null()
    {
        var metadata = new StorageMetadata();

        Assert.Null(metadata.Bucket);
        Assert.Null(metadata.Generation);
        Assert.Null(metadata.MetaGeneration);
        Assert.Null(metadata.Name);
        Assert.Null(metadata.Path);
        Assert.Equal(0, metadata.Size);
        Assert.Null(metadata.CacheControl);
        Assert.Null(metadata.ContentDisposition);
        Assert.Null(metadata.ContentEncoding);
        Assert.Null(metadata.ContentLanguage);
        Assert.Null(metadata.ContentType);
        Assert.Null(metadata.CustomMetadata);
        Assert.Null(metadata.MD5Hash);
        Assert.Null(metadata.StorageReference);
        Assert.Null(metadata.CreationTime);
        Assert.Null(metadata.UpdatedTime);
    }

    [Fact]
    public void constructor_preserves_supplied_values()
    {
        var storageReference = Substitute.For<IStorageReference>();
        var customMetadata = new Dictionary<string, string> { ["key"] = "value" };
        var creationTime = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var updatedTime = new DateTimeOffset(2025, 6, 7, 8, 9, 10, TimeSpan.Zero);

        var metadata = new StorageMetadata(
            bucket: "bucket",
            generation: 11,
            metaGeneration: 12,
            name: "file.txt",
            path: "folder/file.txt",
            size: 13,
            cacheControl: "public,max-age=60",
            contentDisposition: "inline",
            contentEncoding: "identity",
            contentLanguage: "en",
            contentType: "text/plain",
            customMetadata: customMetadata,
            md5Hash: "hash",
            storageReference: storageReference,
            updatedTime: updatedTime,
            creationTime: creationTime);

        Assert.Equal("bucket", metadata.Bucket);
        Assert.Equal(11, metadata.Generation);
        Assert.Equal(12, metadata.MetaGeneration);
        Assert.Equal("file.txt", metadata.Name);
        Assert.Equal("folder/file.txt", metadata.Path);
        Assert.Equal(13, metadata.Size);
        Assert.Equal("public,max-age=60", metadata.CacheControl);
        Assert.Equal("inline", metadata.ContentDisposition);
        Assert.Equal("identity", metadata.ContentEncoding);
        Assert.Equal("en", metadata.ContentLanguage);
        Assert.Equal("text/plain", metadata.ContentType);
        Assert.Same(customMetadata, metadata.CustomMetadata);
        Assert.Equal("hash", metadata.MD5Hash);
        Assert.Same(storageReference, metadata.StorageReference);
        Assert.Equal(creationTime, metadata.CreationTime);
        Assert.Equal(updatedTime, metadata.UpdatedTime);
    }
}