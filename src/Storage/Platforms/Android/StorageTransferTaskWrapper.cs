using Android.Gms.Extensions;
using Firebase.Storage;
using Plugin.Firebase.Android.Common;
using Plugin.Firebase.Android.Storage.Listeners;
using Plugin.Firebase.Storage;
using Object = Java.Lang.Object;
using OnSuccessListener = Plugin.Firebase.Android.Storage.Listeners.OnSuccessListener;

namespace Plugin.Firebase.Android.Storage;

public sealed class StorageTransferTaskWrapper : IStorageTransferTask
{
    private readonly StorageTask _transferTask;
    private readonly IDictionary<Action<IStorageTaskSnapshot>, Object> _observerDict;

    public StorageTransferTaskWrapper(StorageTask transferTask)
    {
        _transferTask = transferTask;
        _observerDict = new Dictionary<Action<IStorageTaskSnapshot>, Object>();
    }

    public Task AwaitAsync()
    {
        return _transferTask.AsAsync();
    }

    public void AddObserver(StorageTaskStatus status, Action<IStorageTaskSnapshot> observer)
    {
        switch(status) {
            case StorageTaskStatus.Pause:
                ObserveStatusPause(observer);
                break;
            case StorageTaskStatus.Progress:
                ObserveStatusProgress(observer);
                break;
            case StorageTaskStatus.Success:
                ObserveStatusSuccess(observer);
                break;
            case StorageTaskStatus.Failure:
                ObserveStatusFailure(observer);
                break;
        }
    }

    private void ObserveStatusPause(Action<IStorageTaskSnapshot> observer)
    {
        var listener = new OnPausedListener(x => observer.Invoke(x.ToAbstract()));
        _observerDict[observer] = listener;
        _transferTask.AddOnPausedListener(listener);
    }

    private void ObserveStatusProgress(Action<IStorageTaskSnapshot> observer)
    {
        var listener = new OnProgressListener(x => observer.Invoke(x.ToAbstract()));
        _observerDict[observer] = listener;
        _transferTask.AddOnProgressListener(listener);
    }

    private void ObserveStatusSuccess(Action<IStorageTaskSnapshot> observer)
    {
        var listener = new OnSuccessListener(x => observer.Invoke(x.ToAbstract()));
        _observerDict[observer] = listener;
        _transferTask.AddOnSuccessListener(listener);
    }

    private void ObserveStatusFailure(Action<IStorageTaskSnapshot> observer)
    {
        var listener = new OnFailureListener(x => observer.Invoke(StorageTaskTaskSnapshotWrapper.FromError(x)));
        _observerDict[observer] = listener;
        _transferTask.AddOnFailureListener(listener);
    }

    public void RemoveObserver(Action<IStorageTaskSnapshot> observer)
    {
        if(_observerDict.ContainsKey(observer)) {
            switch(_observerDict[observer]) {
                case OnPausedListener x:
                    _transferTask.RemoveOnPausedListener(x);
                    break;
                case OnProgressListener x:
                    _transferTask.RemoveOnProgressListener(x);
                    break;
                case OnSuccessListener x:
                    _transferTask.RemoveOnSuccessListener(x);
                    break;
                case OnFailureListener x:
                    _transferTask.RemoveOnFailureListener(x);
                    break;
            }
            _observerDict.Remove(observer);
        }
    }

    public void Pause()
    {
        _transferTask.Pause();
    }

    public void Resume()
    {
        _transferTask.Resume();
    }

    public void Cancel()
    {
        _transferTask.Cancel();
    }
}