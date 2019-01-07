using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Android.Runtime;
using Firebase.Firestore;
using Java.Lang;
using Plugin.Firebase.Abstractions.Common;
using Plugin.Firebase.Abstractions.Firestore;
using Plugin.Firebase.Android.Common;
using Plugin.Firebase.Android.Extensions;
using Exception = System.Exception;
using SetOptions = Plugin.Firebase.Abstractions.Firestore.SetOptions;
using Task = System.Threading.Tasks.Task;

namespace Plugin.Firebase.Android.Firestore
{
    public sealed class DocumentReferenceWrapper : IDocumentReference
    {
        private readonly DocumentReference _reference;
        
        public DocumentReferenceWrapper(DocumentReference reference)
        {
            _reference = reference;
        }

        public async Task SetDataAsync(object data, SetOptions options = null)
        {
            if(options == null) {
                await _reference.Set(data.ToHashMap());
            } else {
                await _reference.Set(data.ToHashMap(), options.ToNative());
            }
        }

        public async Task SetDataAsync(Dictionary<object, object> data, SetOptions options = null)
        {
            if(options == null) {
                await _reference.Set(data.ToHashMap());
            } else {
                await _reference.Set(data.ToHashMap(), options.ToNative());
            }
        }

        public async Task SetDataAsync(params (object, object)[] data)
        {
            await _reference.Set(data.ToHashMap());
        }

        public async Task SetDataAsync(SetOptions options, params (object, object)[] data)
        {
            if(options == null) {
                await _reference.Set(data.ToHashMap());
            } else {
                await _reference.Set(data.ToHashMap(), options.ToNative());
            }
        }

        public async Task UpdateDataAsync(Dictionary<object, object> data)
        {
            await _reference.Update(data.ToJavaObjectDictionary());
        }

        public async Task DeleteDocumentAsync()
        {
            await _reference.Delete();
        }

        public Task<IDocumentSnapshot<T>> GetDocumentSnapshotAsync<T>()
        {
            var tcs = new TaskCompletionSource<IDocumentSnapshot<T>>();
            _reference
                .Get()
                .AddOnCompleteListener(new OnCompleteListener(task => {
                    if(task.IsSuccessful) {
                        var snapshot = task.GetResult(Class.FromType(typeof(DocumentSnapshot))).JavaCast<DocumentSnapshot>();
                        tcs.SetResult(new DocumentSnapshotWrapper<T>(snapshot));
                    } else {
                        tcs.SetException(task.Exception);
                    }
                }));
            return tcs.Task;
        }

        public IDisposable AddSnapshotListener<T>(
            Action<IDocumentSnapshot<T>> onChanged,
            Action<Exception> onError = null,
            bool includeMetaDataChanges = false)
        {
            var registration = _reference
                .AddSnapshotListener(GetDocumentListenOptions(includeMetaDataChanges), new EventListener(
                    x => onChanged(new DocumentSnapshotWrapper<T>(x.JavaCast<DocumentSnapshot>())), 
                    e => onError?.Invoke(new FirebaseException(e.LocalizedMessage))));
            return new DisposableWithAction(registration.Remove);
        }

        private static DocumentListenOptions GetDocumentListenOptions(bool includeMetaDataChanges)
        {
            var options = new DocumentListenOptions();
            if(includeMetaDataChanges) {
                options.IncludeMetadataChanges();
            }
            return options;
        }

        public string Id => _reference.Id;
        public string Path => _reference.Path;
        public ICollectionReference Parent => new CollectionReferenceWrapper(_reference.Parent);
    }
}