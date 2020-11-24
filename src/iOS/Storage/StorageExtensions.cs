using System;
using Firebase.Storage;
using Plugin.Firebase.Storage;
using NativeStorageTaskStatus = Firebase.Storage.StorageTaskStatus;
using StorageTaskStatus = Plugin.Firebase.Storage.StorageTaskStatus;

namespace Plugin.Firebase.iOS.Storage
{
    public static class StorageExtensions
    {
        public static IStorageTaskSnapshot ToAbstract(this StorageTaskSnapshot @this)
        {
            return StorageTaskTaskSnapshotWrapper.FromSnapshot(@this);
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
    }
}