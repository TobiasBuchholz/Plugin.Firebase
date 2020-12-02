using System;
using Firebase.Storage;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.Android.Storage
{
    public sealed class StorageTaskTaskSnapshotWrapper : IStorageTaskSnapshot
    {
        public static IStorageTaskSnapshot FromSnapshot(UploadTask.TaskSnapshot snapshot)
        {
            return new StorageTaskTaskSnapshotWrapper(snapshot:snapshot);
        }

        public static IStorageTaskSnapshot FromError(Exception error)
        {
            return new StorageTaskTaskSnapshotWrapper(error:error);
        }
        
        private StorageTaskTaskSnapshotWrapper(
            UploadTask.TaskSnapshot snapshot = null,
            Exception error = null)
        {
            if(snapshot != null) {
                TransferredUnitCount = snapshot.BytesTransferred;
                TotalUnitCount = snapshot.TotalByteCount;
                TransferredFraction = snapshot.BytesTransferred / (double) snapshot.TotalByteCount;
            }

            Metadata = snapshot?.Metadata?.ToAbstract();
            Error = error;
        }

        public long TransferredUnitCount { get; }
        public long TotalUnitCount { get; }
        public double TransferredFraction { get; }
        public IStorageMetadata Metadata { get; }
        public Exception Error { get; }
    }
}