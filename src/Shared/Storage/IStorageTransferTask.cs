using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Storage
{
    public interface IStorageTransferTask
    {
        Task AwaitAsync();
        void AddObserver(StorageTaskStatus status, Action<IStorageTaskSnapshot> observer);
        void RemoveObserver(Action<IStorageTaskSnapshot> observer);

        void Pause();
        void Resume();
        void Cancel();
    }
}