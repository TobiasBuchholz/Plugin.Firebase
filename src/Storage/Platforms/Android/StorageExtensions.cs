using Firebase.Storage;
using Java.Lang;
using NativeStorageMetadata = Firebase.Storage.StorageMetadata;

namespace Plugin.Firebase.Storage.Platforms.Android;

public static class StorageExtensions
{
    public static IStorageReference ToAbstract(this StorageReference @this)
    {
        return new StorageReferenceWrapper(@this);
    }

    public static IStorageTransferTask ToAbstract(this StorageTask @this)
    {
        return new StorageTransferTaskWrapper(@this);
    }

    public static IStorageTaskSnapshot ToAbstract(this StorageTask.SnapshotBase @this)
    {
        return StorageTaskTaskSnapshotWrapper.FromSnapshot(@this);
    }

    public static IStorageListResult ToAbstract(this ListResult @this)
    {
        return new StorageListResultWrapper(@this);
    }

    public static IStorageMetadata ToAbstract(this NativeStorageMetadata @this)
    {
        return new StorageMetadata(
            bucket: @this.Bucket,
            generation: Long.ParseLong(@this.Generation),
            metaGeneration: Long.ParseLong(@this.MetadataGeneration),
            name: @this.Name,
            path: @this.Path,
            size: @this.SizeBytes,
            cacheControl: @this.CacheControl,
            contentDisposition: @this.ContentDisposition,
            contentEncoding: @this.ContentEncoding,
            contentLanguage: @this.ContentLanguage,
            contentType: @this.ContentType,
            customMetadata: @this.CustomMetadataKeys?.Select(x => (x, @this.GetCustomMetadata(x))).ToDictionary(x => x.Item1, x => x.Item2),
            md5Hash: @this.Md5Hash,
            storageReference: @this.Reference?.ToAbstract(),
            creationTime: DateTimeOffset.FromUnixTimeMilliseconds(@this.CreationTimeMillis),
            updatedTime: DateTimeOffset.FromUnixTimeMilliseconds(@this.UpdatedTimeMillis));
    }

    public static NativeStorageMetadata ToNative(this IStorageMetadata @this)
    {
        var builder = new NativeStorageMetadata.Builder()
            .SetCacheControl(@this.CacheControl)
            .SetContentDisposition(@this.ContentDisposition)
            .SetContentEncoding(@this.ContentEncoding)
            .SetContentLanguage(@this.ContentLanguage)
            .SetContentType(@this.ContentType);

        @this
            .CustomMetadata?
            .ToList()
            .ForEach(x => builder.SetCustomMetadata(x.Key, x.Value));

        return builder.Build();
    }
}