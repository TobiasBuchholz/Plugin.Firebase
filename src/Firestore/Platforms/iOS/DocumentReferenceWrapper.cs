using Firebase.CloudFirestore;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore.Platforms.iOS.Extensions;
using NativeFieldPath = Firebase.CloudFirestore.FieldPath;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

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
        var nsData = data.ToNSObjectDictionary();
        if(options == null) {
            return Wrapped.SetDataAsync(nsData);
        }
        switch(options.Type) {
            case SetOptions.TypeMerge:
                return Wrapped.SetDataAsync(nsData, true);
            case SetOptions.TypeMergeFieldPaths:
                return Wrapped.SetDataAsync(nsData, options.FieldPaths.Select(x => new NativeFieldPath(x.ToArray())).ToArray());
            case SetOptions.TypeMergeFields:
                return Wrapped.SetDataAsync(nsData, options.Fields.ToArray());
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
        return Wrapped.UpdateDataAsync(data.ToNSObjectDictionary());
    }

    public Task UpdateDataAsync(params (string, object)[] data)
    {
        return Wrapped.UpdateDataAsync(data.ToNSObjectDictionary());
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
    public ICollectionReference Parent => Wrapped.Parent == null ? null : new CollectionReferenceWrapper(Wrapped.Parent);
    public DocumentReference Wrapped { get; }
}