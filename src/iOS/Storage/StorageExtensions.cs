using System;
using Firebase.Storage;
using Plugin.Firebase.iOS.Extensions;
using Plugin.Firebase.Storage;
using NativeStorageTaskStatus = Firebase.Storage.StorageTaskStatus;
using NativeStorageMetadata = Firebase.Storage.StorageMetadata;
using StorageMetadata = Plugin.Firebase.Storage.StorageMetadata;
using StorageTaskStatus = Plugin.Firebase.Storage.StorageTaskStatus;

namespace Plugin.Firebase.iOS.Storage
{
    public static class StorageExtensions
    {
        public static IStorageReference ToAbstract(this StorageReference @this)
        {
            return new StorageReferenceWrapper(@this);
        }

        public static IStorageTaskSnapshot ToAbstract(this StorageTaskSnapshot @this)
        {
            return StorageTaskTaskSnapshotWrapper.FromSnapshot(@this);
        }

        public static IStorageListResult ToAbstract(this StorageListResult @this)
        {
            return new StorageListResultWrapper(@this);
        }

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
                    throw new ArgumentException($"Couldn't convert {nameof(StorageTaskStatus)} {@this} to native status");
            }
        }

        public static IStorageMetadata ToAbstract(this NativeStorageMetadata @this)
        {
            return new StorageMetadata(
                bucket:@this.Bucket,
                generation:@this.Generation,
                metaGeneration:@this.Metageneration,
                name:@this.Name,
                path:@this.Path,
                size:@this.Size,
                cacheControl:@this.CacheControl,
                contentDisposition:@this.ContentDisposition,
                contentEncoding:@this.ContentEncoding,
                contentLanguage:@this.ContentLanguage,
                contentType:@this.ContentType,
                customMetadata:@this.CustomMetadata?.ToDictionary(),
                md5Hash:@this.Md5Hash,
                storageReference:@this.StorageReference?.ToAbstract(),
                creationTime:@this.TimeCreated.ToDateTimeOffset(),
                updatedTime:@this.Updated.ToDateTimeOffset());
        }

        public static NativeStorageMetadata ToNative(this IStorageMetadata @this)
        {
            return new NativeStorageMetadata {
                ContentDisposition = @this.ContentDisposition,
                ContentEncoding = @this.ContentEncoding,
                ContentLanguage = @this.ContentLanguage,
                ContentType = @this.ContentType,
                CustomMetadata = @this.CustomMetadata?.ToNSDictionary()
            };
        }
    }
}