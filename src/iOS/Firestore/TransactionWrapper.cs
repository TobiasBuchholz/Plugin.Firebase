using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.CloudFirestore;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.iOS.Extensions;
using FieldPath = Firebase.CloudFirestore.FieldPath;

namespace Plugin.Firebase.iOS.Firestore
{
    public sealed class TransactionWrapper : ITransaction
    {
        private readonly Transaction _wrapped;

        public TransactionWrapper(Transaction wrapped)
        {
            _wrapped = wrapped;
        }

        public override string ToString()
        {
            return _wrapped.ToString();
        }

        public IDocumentSnapshot<T> GetDocument<T>(IDocumentReference document)
        {
            var snapshot = _wrapped.GetDocument(document.ToNative(), out var error).ToAbstract<T>();
            if(error == null) {
                return snapshot;
            } else {
                throw new FirebaseException(error.LocalizedDescription);
            }
        }

        public ITransaction SetData(IDocumentReference document, object data, SetOptions options = null)
        {
            return SetData(document, data.ToDictionary(), options);
        }

        public ITransaction SetData(IDocumentReference document, Dictionary<object, object> data, SetOptions options = null)
        {
            if(options == null) {
                return _wrapped.SetData(data, document.ToNative()).ToAbstract();
            }

            switch(options.Type) {
                case SetOptions.TypeMerge:
                    return _wrapped.SetData(data, document.ToNative(), true).ToAbstract();
                case SetOptions.TypeMergeFieldPaths:
                    return _wrapped.SetData(data, document.ToNative(), options.FieldPaths.Select(x => new FieldPath(x.ToArray())).ToArray()).ToAbstract();
                case SetOptions.TypeMergeFields:
                    return _wrapped.SetData(data, document.ToNative(), options.Fields.ToArray()).ToAbstract();
                default:
                    throw new ArgumentException($"SetOptions type {options.Type} is not supported.");
            }
        }

        public ITransaction SetData(IDocumentReference document, params (object, object)[] data)
        {
            return SetData(document, data.ToDictionary());
        }

        public ITransaction SetData(IDocumentReference document, SetOptions options, params (object, object)[] data)
        {
            return SetData(document, data.ToDictionary(), options);
        }

        public ITransaction UpdateData(IDocumentReference document, Dictionary<object, object> data)
        {
            return _wrapped.UpdateData(data, document.ToNative()).ToAbstract();
        }

        public ITransaction UpdateData(IDocumentReference document, params (string, object)[] data)
        {
            return _wrapped.UpdateData(data.ToNSObjectDictionary(), document.ToNative()).ToAbstract();
        }

        public ITransaction DeleteDocument(IDocumentReference document)
        {
            return _wrapped.DeleteDocument(document.ToNative()).ToAbstract();
        }
    }
}