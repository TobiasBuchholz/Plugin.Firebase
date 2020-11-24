using Firebase.Storage;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.Android.Storage
{
    public static class StorageExtensions
    {
        public static IStorageTaskSnapshot ToAbstract(this UploadTask.TaskSnapshot @this)
        {
            return StorageTaskTaskSnapshotWrapper.FromSnapshot(@this);
        }
    }
}