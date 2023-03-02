using Firebase.Storage;
using Foundation;
using Plugin.Firebase.Storage.Platforms.iOS.Extensions;
using NativeStorageTaskStatus = Firebase.Storage.StorageTaskStatus;

namespace Plugin.Firebase.Storage.Platforms.iOS;

public sealed class StorageTransferTaskWrapper<StorageTransferTask, CompletionResult> : IStorageTransferTask
    where StorageTransferTask : StorageObservableTask, IStorageTaskManagement
{
    private readonly TaskCompletionSource<CompletionResult> _tcs;
    private readonly IDictionary<Action<IStorageTaskSnapshot>, string> _observerDict;

    public StorageTransferTaskWrapper()
    {
        _tcs = new TaskCompletionSource<CompletionResult>();
        _observerDict = new Dictionary<Action<IStorageTaskSnapshot>, string>();

        CompletionHandler = (result, error) => {
            if(error == null) {
                _tcs.SetResult(result);
            } else {
                _tcs.SetException(new Exception(error.LocalizedDescription));
            }
        };
    }

    public Task AwaitAsync()
    {
        return _tcs.Task;
    }

    public void AddObserver(StorageTaskStatus status, Action<IStorageTaskSnapshot> observer)
    {
        if(TransferTask == null) {
            throw new ArgumentException($"You have to set the {nameof(TransferTask)} property before calling this method");
        }

        var handle = TransferTask.ObserveStatus(status.ToNative(), x => observer.Invoke(x.ToAbstract()));
        _observerDict[observer] = handle;
    }

    public void RemoveObserver(Action<IStorageTaskSnapshot> observer)
    {
        if(_observerDict.ContainsKey(observer)) {
            TransferTask.RemoveObserver(_observerDict[observer]);
            _observerDict.Remove(observer);
        }
    }

    public void Pause()
    {
        TransferTask.Pause();
    }

    public void Resume()
    {
        TransferTask.Resume();
    }

    public void Cancel()
    {
        TransferTask.Cancel();
    }

    public delegate void StorageTransferCompletionHandler(CompletionResult result, NSError error);
    public StorageTransferCompletionHandler CompletionHandler { get; }
    public StorageTransferTask TransferTask { private get; set; }
}