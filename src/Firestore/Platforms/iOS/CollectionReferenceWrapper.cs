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
    public async Task<IDocumentReference> AddDocumentAsync(object data)
    {
        var nativeData = data.ToDictionary().ToNSObjectDictionary();
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var documentReference = _wrapped.AddDocument(
            nativeData,
            error => {
                if(error == null) {
                    tcs.TrySetResult();
                } else {
                    tcs.TrySetException(new FirebaseException(error.LocalizedDescription));
                }
            }
        );
        await tcs.Task;
        return new DocumentReferenceWrapper(documentReference);
    }

    /// <inheritdoc/>
    public IDocumentReference? Parent => _wrapped.Parent?.ToAbstract();
}
