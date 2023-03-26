using Android.Runtime;
using Firebase.Storage;
using Object = Java.Lang.Object;

namespace Plugin.Firebase.Storage.Platforms.Android.Listeners;

public sealed class OnProgressListener : Object, IOnProgressListener
{
    private readonly Action<StorageTask.SnapshotBase> _action;

    public OnProgressListener(Action<StorageTask.SnapshotBase> action)
    {
        _action = action;
    }

    public void snapshot(Object snapshot)
    {
        if(snapshot != null) {
            _action.Invoke(snapshot.JavaCast<StorageTask.SnapshotBase>());
        }
    }
}