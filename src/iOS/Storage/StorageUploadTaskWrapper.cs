using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Storage;
using Plugin.Firebase.Storage;
using NativeStorageTaskStatus = Firebase.Storage.StorageTaskStatus;
using StorageTaskStatus = Plugin.Firebase.Storage.StorageTaskStatus;

namespace Plugin.Firebase.iOS.Storage
{
    public sealed class StorageUploadTaskWrapper : IStorageUploadTask
    {
        private readonly TaskCompletionSource<bool> _tcs;
        private readonly IDictionary<Action<IStorageTaskSnapshot>, string> _observerDict;

        public StorageUploadTaskWrapper()
        {
            _tcs = new TaskCompletionSource<bool>();
            _observerDict = new Dictionary<Action<IStorageTaskSnapshot>, string>();
            
            CompletionHandler = (metadata, error) => {
                if(error != null) {
                    _tcs.SetException(new Exception(error.LocalizedDescription));
                } else {
                    _tcs.SetResult(true);
                }
            };
        }

        public Task AwaitAsync()
        {
            return _tcs.Task;
        }

        public void AddObserver(StorageTaskStatus status, Action<IStorageTaskSnapshot> observer)
        {
            if(UploadTask == null) {
                throw new ArgumentException($"You have to set the {nameof(UploadTask)} property before calling this method");
            }

            var handle = UploadTask.ObserveStatus(status.ToNative(), x => observer.Invoke(x.ToAbstract()));
            _observerDict[observer] = handle;
        }

        public void RemoveObserver(Action<IStorageTaskSnapshot> observer)
        {
            if(_observerDict.ContainsKey(observer)) {
                UploadTask.RemoveObserver(_observerDict[observer]);
                _observerDict.Remove(observer);
            }
        }

        public StorageGetPutUpdateCompletionHandler CompletionHandler { get; }
        public StorageUploadTask UploadTask { private get; set; }
    }
}