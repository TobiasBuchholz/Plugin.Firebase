using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Storage;
using Plugin.Firebase.Android.Common;
using Plugin.Firebase.Android.Storage.Listeners;
using Plugin.Firebase.Extensions;
using Plugin.Firebase.Storage;
using Object = Java.Lang.Object;
using OnSuccessListener = Plugin.Firebase.Android.Storage.Listeners.OnSuccessListener;

namespace Plugin.Firebase.Android.Storage
{
    public sealed class StorageUploadTaskWrapper : IStorageUploadTask
    {
        private readonly UploadTask _uploadTask;
        private readonly IDictionary<Action<IStorageTaskSnapshot>, Object> _observerDict;

        public StorageUploadTaskWrapper(UploadTask uploadTask)
        {
            _uploadTask = uploadTask;
            _observerDict = new Dictionary<Action<IStorageTaskSnapshot>, Object>();
        }

        public Task AwaitAsync()
        {
            return _uploadTask.ToTask();
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
            _uploadTask.AddOnPausedListener(listener);
        }
        
        private void ObserveStatusProgress(Action<IStorageTaskSnapshot> observer)
        {
            var listener = new OnProgressListener(x => observer.Invoke(x.ToAbstract()));
            _observerDict[observer] = listener;
            _uploadTask.AddOnProgressListener(listener);
        }
        
        private void ObserveStatusSuccess(Action<IStorageTaskSnapshot> observer)
        {
            var listener = new OnSuccessListener(x => observer.Invoke(x.ToAbstract()));
            _observerDict[observer] = listener;
            _uploadTask.AddOnSuccessListener(listener);
        }
        
        private void ObserveStatusFailure(Action<IStorageTaskSnapshot> observer)
        {
            var listener = new OnFailureListener(x => observer.Invoke(StorageTaskTaskSnapshotWrapper.FromError(x)));
            _observerDict[observer] = listener;
            _uploadTask.AddOnFailureListener(listener);
        }

        public void RemoveObserver(Action<IStorageTaskSnapshot> observer)
        {
            if(_observerDict.ContainsKey(observer)) {
                switch(_observerDict[observer]) {
                    case OnPausedListener x:
                        _uploadTask.RemoveOnPausedListener(x);
                        break;
                    case OnProgressListener x:
                        _uploadTask.RemoveOnProgressListener(x);
                        break;
                    case OnSuccessListener x:
                        _uploadTask.RemoveOnSuccessListener(x);
                        break;
                    case OnFailureListener x:
                        _uploadTask.RemoveOnFailureListener(x);
                        break;
                }
                _observerDict.Remove(observer);
            }
        }
    }
}