using System;
using System.Collections.Generic;
using Android.Gms.Extensions;
using Firebase.Firestore;
using Plugin.Firebase.Abstractions.Firestore;
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

        public Task SetDataAsync(object data, SetOptions options = null)
        {
            return SetDataAsync(data.ToDictionary<object>(), options);
        }

        public async Task SetDataAsync(Dictionary<object, object> data, SetOptions options = null)
        {
            if(options == null) {
                await _reference.Set(data.ToHashMap());
            } else {
                await _reference.Set(data.ToHashMap(), options.ToNative());
            }
        }

        public Task SetDataAsync(params (object, object)[] data)
        {
            return SetDataAsync(data.ToDictionary());
        }

        public Task SetDataAsync(SetOptions options, params (object, object)[] data)
        {
            return SetDataAsync(data.ToDictionary(), options);
        }

        public async Task UpdateDataAsync(Dictionary<object, object> data)
        {
            await _reference.Update(data.ToJavaObjectDictionary());
        }

        public async Task DeleteDocumentAsync()
        {
            await _reference.Delete();
        }

        public string Id => _reference.Id;
        public string Path => _reference.Path;
        public ICollectionReference Parent => new CollectionReferenceWrapper(_reference.Parent);
    }
}