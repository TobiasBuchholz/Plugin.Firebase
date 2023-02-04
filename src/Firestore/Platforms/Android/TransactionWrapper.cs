using Firebase.Firestore;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Firestore.Android.Extensions;
using SetOptions = Plugin.Firebase.Firestore.SetOptions;

namespace Plugin.Firebase.Android.Firestore;

public sealed class TransactionWrapper : ITransaction
{
    private readonly Transaction _wrapped;

    public TransactionWrapper(Transaction wrapped)
    {
        _wrapped = wrapped;
    }

    public override string ToString()
    {
        return _wrapped?.ToString() ?? "null";
    }

    public IDocumentSnapshot<T> GetDocument<T>(IDocumentReference document)
    {
        return _wrapped.Get(document.ToNative()).ToAbstract<T>();
    }

    public ITransaction SetData(IDocumentReference document, object data, SetOptions options = null)
    {
        return options == null
            ? _wrapped.Set(document.ToNative(), data.ToHashMap()).ToAbstract()
            : _wrapped.Set(document.ToNative(), data.ToHashMap(), options.ToNative()).ToAbstract();
    }

    public ITransaction SetData(IDocumentReference document, Dictionary<object, object> data, SetOptions options = null)
    {
        return options == null
            ? _wrapped.Set(document.ToNative(), data.ToHashMap()).ToAbstract()
            : _wrapped.Set(document.ToNative(), data.ToHashMap(), options.ToNative()).ToAbstract();
    }

    public ITransaction SetData(IDocumentReference document, params (object, object)[] data)
    {
        return _wrapped.Set(document.ToNative(), data.ToHashMap()).ToAbstract();
    }

    public ITransaction SetData(IDocumentReference document, SetOptions options, params (object, object)[] data)
    {
        return options == null
            ? _wrapped.Set(document.ToNative(), data.ToHashMap()).ToAbstract()
            : _wrapped.Set(document.ToNative(), data.ToHashMap(), options.ToNative()).ToAbstract();
    }

    public ITransaction UpdateData(IDocumentReference document, Dictionary<object, object> data)
    {
        return _wrapped.Update(document.ToNative(), data.ToJavaObjectDictionary()).ToAbstract();
    }

    public ITransaction UpdateData(IDocumentReference document, params (string, object)[] data)
    {
        return _wrapped.Update(document.ToNative(), data.ToJavaObjectDictionary()).ToAbstract();
    }

    public ITransaction DeleteDocument(IDocumentReference document)
    {
        return _wrapped.Delete(document.ToNative()).ToAbstract();
    }
}