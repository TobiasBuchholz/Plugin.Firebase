using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Android.Runtime;
using Firebase.Firestore;
using Java.Lang;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Android.Common;
using Exception = System.Exception;
using SetOptions = Plugin.Firebase.Firestore.SetOptions;
using Task = System.Threading.Tasks.Task;

namespace Plugin.Firebase.Android.Firestore
{
    public sealed class DocumentReferenceWrapper : IDocumentReference
    {
        public DocumentReferenceWrapper(DocumentReference reference)
        {
            Wrapped = reference;
        }

        public async Task SetDataAsync(object data, SetOptions options = null)
        {
            if(options == null) {
                await Wrapped.Set(data.ToHashMap());
            } else {
                await Wrapped.Set(data.ToHashMap(), options.ToNative());
            }
        }

        public async Task SetDataAsync(Dictionary<object, object> data, SetOptions options = null)
        {
            if(options == null) {
                await Wrapped.Set(data.ToHashMap());
            } else {
                await Wrapped.Set(data.ToHashMap(), options.ToNative());
            }
        }

        public async Task SetDataAsync(params (object, object)[] data)
        {
            await Wrapped.Set(data.ToHashMap());
        }

        public async Task SetDataAsync(SetOptions options, params (object, object)[] data)
        {
            if(options == null) {
                await Wrapped.Set(data.ToHashMap());
            } else {
                await Wrapped.Set(data.ToHashMap(), options.ToNative());
            }
        }

        public async Task UpdateDataAsync(Dictionary<object, object> data)
        {
            await Wrapped.Update(data.ToJavaObjectDictionary());
        }

        public async Task UpdateDataAsync(params (string, object)[] data)
        {
            await Wrapped.Update(data.ToJavaObjectDictionary());
        }

        public async Task DeleteDocumentAsync()
        {
            await Wrapped.Delete();
        }

        public Task<IDocumentSnapshot<T>> GetDocumentSnapshotAsync<T>()
        {
            var tcs = new TaskCompletionSource<IDocumentSnapshot<T>>();
            Wrapped
                .Get()
                .AddOnCompleteListener(new OnCompleteListener(task => {
                    if(task.IsSuccessful) {
                        var snapshot = task.GetResult(Class.FromType(typeof(DocumentSnapshot))).JavaCast<DocumentSnapshot>();
                        tcs.SetResult(snapshot.ToAbstract<T>());
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
            var registration = Wrapped
                .AddSnapshotListener(includeMetaDataChanges ? MetadataChanges.Include : MetadataChanges.Exclude, new EventListener(
                    x => onChanged(x.JavaCast<DocumentSnapshot>().ToAbstract<T>()), 
                    e => onError?.Invoke(new FirebaseException(e.LocalizedMessage))));
            return new DisposableWithAction(registration.Remove);
        }

        public string Id => Wrapped.Id;
        public string Path => Wrapped.Path;
        public ICollectionReference Parent => new CollectionReferenceWrapper(Wrapped.Parent);
        public DocumentReference Wrapped { get; }
    }
}