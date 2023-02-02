using Android.Gms.Tasks;
using Android.Runtime;
using Firebase.Storage;
using Object = Java.Lang.Object;

namespace Plugin.Firebase.Android.Storage.Listeners;

public sealed class OnSuccessListener : Object, IOnSuccessListener
{
    private readonly Action<StorageTask.SnapshotBase> _action;

    public OnSuccessListener(Action<StorageTask.SnapshotBase> action)
    {
        _action = action;
    }

    public void OnSuccess(Object snapshot)
    {
        if(snapshot != null) {
            _action.Invoke(snapshot.JavaCast<StorageTask.SnapshotBase>());
        }
    }
}