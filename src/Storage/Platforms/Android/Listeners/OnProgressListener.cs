using System;
using Android.Runtime;
using Firebase.Storage;
using Object = Java.Lang.Object;

namespace Plugin.Firebase.Android.Storage.Listeners
{
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
}