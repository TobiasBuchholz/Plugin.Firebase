using Firebase.Storage;
using Plugin.Firebase.Core.Platforms.iOS.Extensions;
using NativeStorageMetadata = Firebase.Storage.StorageMetadata;
using NativeStorageTaskStatus = Firebase.Storage.StorageTaskStatus;

namespace Plugin.Firebase.Storage.Platforms.iOS.Extensions;

/// <summary>
/// Provides extension methods for converting between abstract and native iOS Firebase Storage types.
/// </summary>
public static class StorageExtensions
{
    /// <summary>
    /// Converts a native iOS StorageReference to an abstract IStorageReference.
    /// </summary>
    /// <param name="this">The native storage reference to convert.</param>
    /// <returns>An abstract storage reference wrapper.</returns>
    public static IStorageReference ToAbstract(this StorageReference @this)
    {
        return new StorageReferenceWrapper(@this);
    }

    /// <summary>
    /// Converts a native iOS StorageTaskSnapshot to an abstract IStorageTaskSnapshot.
    /// </summary>
    /// <param name="this">The native snapshot to convert.</param>
    /// <returns>An abstract storage task snapshot.</returns>
    public static IStorageTaskSnapshot ToAbstract(this StorageTaskSnapshot @this)
    {
        return StorageTaskTaskSnapshotWrapper.FromSnapshot(@this);
    }

    /// <summary>
    /// Converts a native iOS StorageListResult to an abstract IStorageListResult.
    /// </summary>
    /// <param name="this">The native list result to convert.</param>
    /// <returns>An abstract storage list result wrapper.</returns>
    public static IStorageListResult ToAbstract(this StorageListResult @this)
    {
        return new StorageListResultWrapper(@this);
    }

    /// <summary>
    /// Converts an abstract StorageTaskStatus to a native iOS StorageTaskStatus.
    /// </summary>
    /// <param name="this">The abstract status to convert.</param>
    /// <returns>The corresponding native iOS storage task status.</returns>
    /// <exception cref="ArgumentException">Thrown when the status cannot be converted.</exception>
    public static NativeStorageTaskStatus ToNative(this StorageTaskStatus @this)
    {
        switch(@this) {
            case StorageTaskStatus.Unknown:
                return NativeStorageTaskStatus.Unknown;
            case StorageTaskStatus.Progress:
                return NativeStorageTaskStatus.Progress;
            case StorageTaskStatus.Pause:
                return NativeStorageTaskStatus.Pause;
            case StorageTaskStatus.Success:
                return NativeStorageTaskStatus.Success;
            case StorageTaskStatus.Failure:
                return NativeStorageTaskStatus.Failure;
            default:
                throw new ArgumentException(
                    $"Couldn't convert {nameof(StorageTaskStatus)} {@this} to native status"
                );
        }
    }

    /// <summary>
    /// Converts native iOS StorageMetadata to an abstract IStorageMetadata.
    /// </summary>
    /// <param name="this">The native metadata to convert.</param>
    /// <returns>An abstract storage metadata object.</returns>
    public static IStorageMetadata ToAbstract(this NativeStorageMetadata @this)
    {
        return new StorageMetadata(
            bucket: @this.Bucket,
            generation: @this.Generation,
            metaGeneration: @this.Metageneration,
            name: @this.Name,
            path: @this.Path,
            size: @this.Size,
            cacheControl: @this.CacheControl,
            contentDisposition: @this.ContentDisposition,
            contentEncoding: @this.ContentEncoding,
            contentLanguage: @this.ContentLanguage,
            contentType: @this.ContentType,
            customMetadata: @this.CustomMetadata?.ToDictionary(),
            md5Hash: @this.Md5Hash,
            creationTime: @this.TimeCreated.ToDateTimeOffset(),
            updatedTime: @this.Updated.ToDateTimeOffset()
        );
    }

    /// <summary>
    /// Converts abstract IStorageMetadata to native iOS StorageMetadata.
    /// </summary>
    /// <param name="this">The abstract metadata to convert.</param>
    /// <returns>Native iOS storage metadata.</returns>
    public static NativeStorageMetadata ToNative(this IStorageMetadata @this)
    {
        return new NativeStorageMetadata {
            ContentDisposition = @this.ContentDisposition,
            ContentEncoding = @this.ContentEncoding,
            ContentLanguage = @this.ContentLanguage,
            ContentType = @this.ContentType,
            CustomMetadata = @this.CustomMetadata?.ToNSDictionary(),
        };
    }
}