using Firebase.CloudFirestore;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore.Platforms.iOS.Extensions;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firestore collection reference.
/// </summary>
public sealed class CollectionReferenceWrapper : QueryWrapper, ICollectionReference
{
    private readonly CollectionReference _wrapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionReferenceWrapper"/> class.
    /// </summary>
    /// <param name="reference">The native iOS collection reference to wrap.</param>
    public CollectionReferenceWrapper(CollectionReference reference)
        : base(reference)
    {
        _wrapped = reference;
    }

    /// <inheritdoc/>
    public IDocumentReference GetDocument(string documentPath)
    {
        return new DocumentReferenceWrapper(_wrapped.GetDocument(documentPath));
    }

    /// <inheritdoc/>
    public IDocumentReference CreateDocument()
    {
        return new DocumentReferenceWrapper(_wrapped.CreateDocument());
    }

    /// <inheritdoc/>
    public Task<IDocumentReference> AddDocumentAsync(object data)
    {
        var tcs = new TaskCompletionSource<IDocumentReference>();
        DocumentReference documentReference = null;
        documentReference = _wrapped.AddDocument(
            data.ToDictionary(),
            error => {
                if(error == null) {
                    tcs.SetResult(new DocumentReferenceWrapper(documentReference));
                } else {
                    tcs.SetException(new FirebaseException(error.LocalizedDescription));
                }
            }
        );
        return tcs.Task;
    }

    /// <inheritdoc/>
    public IDocumentReference Parent => _wrapped.Parent?.ToAbstract();
}