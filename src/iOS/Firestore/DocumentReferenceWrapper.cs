using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.CloudFirestore;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.iOS.Extensions;
using FieldPath = Firebase.CloudFirestore.FieldPath;
using FieldValue = Plugin.Firebase.Firestore.FieldValue;

namespace Plugin.Firebase.iOS.Firestore
{
    public sealed class DocumentReferenceWrapper : IDocumentReference
    {
        public DocumentReferenceWrapper(DocumentReference reference)
        {
            Wrapped = reference;
        }

        public Task SetDataAsync(object data, SetOptions options = null)
        {
            return SetDataAsync(data.ToDictionary(), options);
        }

        public Task SetDataAsync(Dictionary<object, object> data, SetOptions options)
        {
            if(options == null) {
                return Wrapped.SetDataAsync(data);
            }

            switch(options.Type) {
                case SetOptions.TypeMerge:
                    return Wrapped.SetDataAsync(data, true);
                case SetOptions.TypeMergeFieldPaths:
                    return Wrapped.SetDataAsync(data, options.FieldPaths.Select(x => new FieldPath(x.ToArray())).ToArray());
                case SetOptions.TypeMergeFields:
                    return Wrapped.SetDataAsync(data, options.Fields.ToArray());
                default:
                    throw new ArgumentException($"SetOptions type {options.Type} is not supported.");
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

        public Task UpdateDataAsync(Dictionary<object, object> data)
        {
            var nativeData = data.ToDictionary(x => x.Key, x => {
                if(x.Value is FieldValue fieldValue) {
                    return fieldValue.ToNative();
                }
                return x.Value;
            });
            return Wrapped.UpdateDataAsync(nativeData);
        }

        public Task UpdateDataAsync(params (string, object)[] data)
        {
            var dict = new Dictionary<object, object>();
            data.ToList().ForEach(x => {
                if(x.Item2 is FieldValue fieldValue) {
                    dict[x.Item1] = fieldValue.ToNative();
                } else {
                    dict[x.Item1] = x.Item2;
                }
            });
            return Wrapped.UpdateDataAsync(dict);
        }

        public Task DeleteDocumentAsync()
        {
            return Wrapped.DeleteDocumentAsync();
        }

        public Task<IDocumentSnapshot<T>> GetDocumentSnapshotAsync<T>(Source source = Source.Default)
        {
            var tcs = new TaskCompletionSource<IDocumentSnapshot<T>>();
            Wrapped.GetDocument(source.ToNative(), (snapshot, error) => {
                if(error == null) {
                    tcs.SetResult(snapshot.ToAbstract<T>());
                } else {
                    tcs.SetException(new FirebaseException(error.LocalizedDescription));
                }
            });
            return tcs.Task;
        }

        public IDisposable AddSnapshotListener<T>(
            Action<IDocumentSnapshot<T>> onChanged,
            Action<Exception> onError = null,
            bool includeMetaDataChanges = false)
        {
            var registration = Wrapped.AddSnapshotListener(includeMetaDataChanges, (snapshot, error) => {
                if(error == null) {
                    onChanged(snapshot.ToAbstract<T>());
                } else {
                    onError?.Invoke(new FirebaseException(error.LocalizedDescription));
                }
            });
            return new DisposableWithAction(registration.Remove);
        }

        public ICollectionReference GetCollection(string collectionPath)
        {
            return Wrapped.GetCollection(collectionPath).ToAbstract();
        }

        public string Id => Wrapped.Id;
        public string Path => Wrapped.Path;
        public ICollectionReference Parent => new CollectionReferenceWrapper(Wrapped.Parent);
        public DocumentReference Wrapped { get; }
    }
}