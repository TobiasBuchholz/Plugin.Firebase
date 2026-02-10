using Firebase.CloudFirestore;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore.Platforms.iOS.Extensions;
using NativeFieldPath = Firebase.CloudFirestore.FieldPath;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

/// <summary>
/// Wraps a native iOS Firestore transaction.
/// </summary>
public sealed class TransactionWrapper : ITransaction
{
    private readonly Transaction _wrapped;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionWrapper"/> class.
    /// </summary>
    /// <param name="wrapped">The native iOS transaction to wrap.</param>
    public TransactionWrapper(Transaction wrapped)
    {
        _wrapped = wrapped;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _wrapped.ToString();
    }

    /// <inheritdoc/>
    public IDocumentSnapshot<T> GetDocument<T>(IDocumentReference document)
    {
        var snapshot = _wrapped.GetDocument(document.ToNative(), out var error).ToAbstract<T>();
        if(error == null) {
            return snapshot;
        } else {
            throw new FirebaseException(error.LocalizedDescription);
        }
    }

    /// <inheritdoc/>
    public ITransaction SetData(IDocumentReference document, object data, SetOptions options = null)
    {
        return SetData(document, data.ToDictionary(), options);
    }

    /// <inheritdoc/>
    public ITransaction SetData(
        IDocumentReference document,
        Dictionary<object, object> data,
        SetOptions options = null
    )
    {
        if(options == null) {
            return _wrapped.SetData(data, document.ToNative()).ToAbstract();
        }

        switch(options.Type) {
            case SetOptions.TypeMerge:
                return _wrapped.SetData(data, document.ToNative(), true).ToAbstract();
            case SetOptions.TypeMergeFieldPaths:
                return _wrapped
                    .SetData(
                        data,
                        document.ToNative(),
                        options.FieldPaths.Select(x => new NativeFieldPath(x.ToArray())).ToArray()
                    )
                    .ToAbstract();
            case SetOptions.TypeMergeFields:
                return _wrapped
                    .SetData(data, document.ToNative(), options.Fields.ToArray())
                    .ToAbstract();
            default:
                throw new ArgumentException($"SetOptions type {options.Type} is not supported.");
        }
    }

    /// <inheritdoc/>
    public ITransaction SetData(IDocumentReference document, params (object, object)[] data)
    {
        return SetData(document, data.ToDictionary());
    }

    /// <inheritdoc/>
    public ITransaction SetData(
        IDocumentReference document,
        SetOptions options,
        params (object, object)[] data
    )
    {
        return SetData(document, data.ToDictionary(), options);
    }

    /// <inheritdoc/>
    public ITransaction UpdateData(IDocumentReference document, Dictionary<object, object> data)
    {
        return _wrapped.UpdateData(data, document.ToNative()).ToAbstract();
    }

    /// <inheritdoc/>
    public ITransaction UpdateData(IDocumentReference document, params (string, object)[] data)
    {
        return _wrapped.UpdateData(data.ToNSObjectDictionary(), document.ToNative()).ToAbstract();
    }

    /// <inheritdoc/>
    public ITransaction DeleteDocument(IDocumentReference document)
    {
        return _wrapped.DeleteDocument(document.ToNative()).ToAbstract();
    }
}