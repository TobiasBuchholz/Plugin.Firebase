using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Firebase.Firestore;
using Plugin.Firebase.Firestore;
using SetOptions = Plugin.Firebase.Firestore.SetOptions;

namespace Plugin.Firebase.Android.Firestore
{
    public sealed class WriteBatchWrapper : IWriteBatch
    {
        private readonly WriteBatch _wrapped;

        public WriteBatchWrapper(WriteBatch writeBatch)
        {
            _wrapped = writeBatch;
        }

        public override string ToString()
        {
            return _wrapped?.ToString() ?? "null";
        }

        public IWriteBatch SetData(IDocumentReference document, object data, SetOptions options = null)
        {
            return options == null
                ? _wrapped.Set(document.ToNative(), data.ToHashMap()).ToAbstract()
                : _wrapped.Set(document.ToNative(), data.ToHashMap(), options.ToNative()).ToAbstract();
        }

        public IWriteBatch SetData(IDocumentReference document, Dictionary<object, object> data, SetOptions options = null)
        {
            return options == null
                ? _wrapped.Set(document.ToNative(), data.ToHashMap()).ToAbstract()
                : _wrapped.Set(document.ToNative(), data.ToHashMap(), options.ToNative()).ToAbstract();
        }

        public IWriteBatch SetData(IDocumentReference document, params (object, object)[] data)
        {
            return _wrapped.Set(document.ToNative(), data.ToHashMap()).ToAbstract();
        }

        public IWriteBatch SetData(IDocumentReference document, SetOptions options, params (object, object)[] data)
        {
            return options == null
                ? _wrapped.Set(document.ToNative(), data.ToHashMap()).ToAbstract()
                : _wrapped.Set(document.ToNative(), data.ToHashMap(), options.ToNative()).ToAbstract();
        }

        public IWriteBatch UpdateData(IDocumentReference document, Dictionary<object, object> data)
        {
            return _wrapped.Update(document.ToNative(), data.ToJavaObjectDictionary()).ToAbstract();
        }

        public IWriteBatch UpdateData(IDocumentReference document, params (string, object)[] data)
        {
            return _wrapped.Update(document.ToNative(), data.ToJavaObjectDictionary()).ToAbstract();
        }

        public IWriteBatch DeleteDocument(IDocumentReference document)
        {
            return _wrapped.Delete(document.ToNative()).ToAbstract();
        }

        public Task CommitAsync()
        {
            return _wrapped.Commit().AsAsync();
        }
    }
}