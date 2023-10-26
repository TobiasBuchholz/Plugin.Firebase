using Firebase.CloudFirestore;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

public sealed class CollectionReferenceWrapper : QueryWrapper, ICollectionReference
{
    private readonly CollectionReference _wrapped;

    public CollectionReferenceWrapper(CollectionReference reference)
        : base(reference)
    {
        _wrapped = reference;
    }

    public IDocumentReference GetDocument(string documentPath)
    {
        return new DocumentReferenceWrapper(_wrapped.GetDocument(documentPath));
    }

    public IDocumentReference CreateDocument()
    {
        return new DocumentReferenceWrapper(_wrapped.CreateDocument());
    }

    public Task<IDocumentReference> AddDocumentAsync(object data)
    {
        var tcs = new TaskCompletionSource<IDocumentReference>();
        DocumentReference documentReference = null;
        documentReference = _wrapped.AddDocument(data.ToDictionary(), error => {
            if(error == null) {
                tcs.SetResult(new DocumentReferenceWrapper(documentReference));
            } else {
                tcs.SetException(new FirebaseException(error.LocalizedDescription));
            }
        });
        return tcs.Task;
    }

    public IDocumentReference Parent => _wrapped.Parent?.ToAbstract();
}
