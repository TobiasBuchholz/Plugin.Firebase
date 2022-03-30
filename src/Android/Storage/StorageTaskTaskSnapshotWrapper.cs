using System;
using Firebase.Storage;
using Plugin.Firebase.Common;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.Android.Storage
{
    public sealed class StorageTaskTaskSnapshotWrapper : IStorageTaskSnapshot
    {
        public static IStorageTaskSnapshot FromSnapshot(StorageTask.SnapshotBase snapshot)
        {
            switch(snapshot) {
                case UploadTask.TaskSnapshot x:
                    return new StorageTaskTaskSnapshotWrapper(x);
                case FileDownloadTask.TaskSnapshot x:
                    return new StorageTaskTaskSnapshotWrapper(x);
                default:
                    throw new FirebaseException($"Couldn't wrap unsupported StorageTask.SnapshotBase {snapshot}");
            }
        }

        public static IStorageTaskSnapshot FromError(Exception error)
        {
            return new StorageTaskTaskSnapshotWrapper(error: error);
        }

        private StorageTaskTaskSnapshotWrapper(
            UploadTask.TaskSnapshot snapshot = null,
            Exception error = null)
            : this(error)
        {
            if(snapshot != null) {
                TransferredUnitCount = snapshot.BytesTransferred;
                TotalUnitCount = snapshot.TotalByteCount;
                TransferredFraction = snapshot.BytesTransferred / (double) snapshot.TotalByteCount;
            }

            Metadata = snapshot?.Metadata?.ToAbstract();
        }

        private StorageTaskTaskSnapshotWrapper(
            FileDownloadTask.TaskSnapshot snapshot = null,
            Exception error = null)
            : this(error)
        {
            if(snapshot != null) {
                TransferredUnitCount = snapshot.BytesTransferred;
                TotalUnitCount = snapshot.TotalByteCount;
                TransferredFraction = snapshot.BytesTransferred / (double) snapshot.TotalByteCount;
            }
        }

        private StorageTaskTaskSnapshotWrapper(Exception error = null)
        {
            Error = error;
        }

        public long TransferredUnitCount { get; }
        public long TotalUnitCount { get; }
        public double TransferredFraction { get; }
        public IStorageMetadata Metadata { get; }
        public Exception Error { get; }
    }
}