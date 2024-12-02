using Firebase.CloudFirestore;
using Plugin.Firebase.Firestore.Platforms.iOS.Extensions;
using NativeFieldPath = Firebase.CloudFirestore.FieldPath;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

public sealed class WriteBatchWrapper : IWriteBatch
{
    private readonly WriteBatch _wrapped;

    public WriteBatchWrapper(WriteBatch writeBatch)
    {
        _wrapped = writeBatch;
    }

    public override string ToString()
    {
        return _wrapped.ToString();
    }

    public IWriteBatch SetData(IDocumentReference document, object data, SetOptions options = null)
    {
        return SetData(document, data.ToDictionary(), options);
    }

    public IWriteBatch SetData(IDocumentReference document, Dictionary<object, object> data, SetOptions options = null)
    {
        if(options == null) {
            return _wrapped.SetData(data, document.ToNative()).ToAbstract();
        }

        switch(options.Type) {
            case SetOptions.TypeMerge:
                return _wrapped.SetData(data, document.ToNative(), true).ToAbstract();
            case SetOptions.TypeMergeFieldPaths:
                return _wrapped.SetData(data, document.ToNative(), options.FieldPaths.Select(x => new NativeFieldPath(x.ToArray())).ToArray()).ToAbstract();
            case SetOptions.TypeMergeFields:
                return _wrapped.SetData(data, document.ToNative(), options.Fields.ToArray()).ToAbstract();
            default:
                throw new ArgumentException($"SetOptions type {options.Type} is not supported.");
        }
    }

    public IWriteBatch SetData(IDocumentReference document, params (object, object)[] data)
    {
        return SetData(document, data.ToDictionary());
    }

    public IWriteBatch SetData(IDocumentReference document, SetOptions options, params (object, object)[] data)
    {
        return SetData(document, data.ToDictionary());
    }

    public IWriteBatch UpdateData(IDocumentReference document, Dictionary<object, object> data)
    {
        return _wrapped.UpdateData(data.ToNSObjectDictionary(), document.ToNative()).ToAbstract();
    }

    public IWriteBatch UpdateData(IDocumentReference document, params (string, object)[] data)
    {
        return _wrapped.UpdateData(data.ToNSObjectDictionary(), document.ToNative()).ToAbstract();
    }

    public IWriteBatch DeleteDocument(IDocumentReference document)
    {
        return _wrapped.DeleteDocument(document.ToNative()).ToAbstract();
    }

    public Task CommitAsync()
    {
        return _wrapped.CommitAsync();
    }

    public void CommitLocal()
    {
        _wrapped.Commit();
    }
}