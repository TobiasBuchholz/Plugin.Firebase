using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Android.Runtime;
using Firebase.Firestore;
using Java.Lang;
using Plugin.Firebase.Abstractions.Firestore;
using Plugin.Firebase.Android.Common;
using Plugin.Firebase.Android.Extensions;
using SetOptions = Plugin.Firebase.Abstractions.Firestore.SetOptions;
using Task = System.Threading.Tasks.Task;

namespace Plugin.Firebase.Firestore
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

        public Task<T> GetDocumentSnapshotAsync<T>()
        {
            var tcs = new TaskCompletionSource<T>();
            _reference
                .Get()
                .AddOnCompleteListener(new OnCompleteListener(task => {
                    if(task.IsSuccessful) {
                        var snapshot = task.GetResult(Class.FromType(typeof(DocumentSnapshot))).JavaCast<DocumentSnapshot>();
                        tcs.SetResult(snapshot.Data.Cast<T>());
                    } else {
                        tcs.SetException(task.Exception);
                    }
                }));
            return tcs.Task;
        }

        public string Id => _reference.Id;
        public string Path => _reference.Path;
        public ICollectionReference Parent => new CollectionReferenceWrapper(_reference.Parent);
    }
}