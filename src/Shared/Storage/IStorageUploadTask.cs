using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Storage
{
    public interface IStorageUploadTask
    {
        Task AwaitAsync();
        void AddObserver(StorageTaskStatus status, Action<IStorageTaskSnapshot> observer);
        void RemoveObserver(Action<IStorageTaskSnapshot> observer);
    }
}