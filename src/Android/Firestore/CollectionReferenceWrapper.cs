using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Android.Runtime;
using Firebase.Firestore;
using Plugin.Firebase.Android.Extensions;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Common;

namespace Plugin.Firebase.Android.Firestore
{
    public sealed class CollectionReferenceWrapper : ICollectionReference
    {
        private readonly CollectionReference _wrapped;
        
        public CollectionReferenceWrapper(CollectionReference reference)
        {
            _wrapped = reference;
        }
        
        public IDisposable AddSnapshotListener<T>(Action<IQuerySnapshot<T>> onChanged, Action<Exception> onError = null, bool includeMetaDataChanges = false)
        {
            var registration = _wrapped
                .AddSnapshotListener(includeMetaDataChanges ? MetadataChanges.Include : MetadataChanges.Exclude, new EventListener(
                    x => onChanged(new QuerySnapshotWrapper<T>(x.JavaCast<QuerySnapshot>())), 
                    e => onError?.Invoke(new FirebaseException(e.LocalizedMessage))));
            return new DisposableWithAction(registration.Remove);
        }

        public IDocumentReference GetDocument(string documentPath)
        {
            return new DocumentReferenceWrapper(_wrapped.Document(documentPath));
        }

        public IDocumentReference CreateDocument()
        {
            return new DocumentReferenceWrapper(_wrapped.Document());
        }

        public IQuery WhereEqualsTo(string field, object value)
        {
            return new QueryWrapper(_wrapped.WhereEqualTo(field, value.ToJavaObject()));
        }

        public IQuery WhereGreaterThan(string field, object value)
        {
            return new QueryWrapper(_wrapped.WhereGreaterThan(field, value.ToJavaObject()));
        }

        public IQuery WhereLessThan(string field, object value)
        {
            return new QueryWrapper(_wrapped.WhereLessThan(field, value.ToJavaObject()));
        }

        public IQuery WhereGreaterThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_wrapped.WhereGreaterThanOrEqualTo(field, value.ToJavaObject()));
        }

        public IQuery WhereLessThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_wrapped.WhereLessThanOrEqualTo(field, value.ToJavaObject()));
        }

        public IQuery OrderBy(string field)
        {
            return new QueryWrapper(_wrapped.OrderBy(field));
        }

        public IQuery StartingAt(object[] fieldValues)
        {
            return new QueryWrapper(_wrapped.StartAt(fieldValues.Select(x => x.ToJavaObject()).ToArray()));
        }

        public IQuery EndingAt(object[] fieldValues)
        {
            return new QueryWrapper(_wrapped.EndAt(fieldValues.Select(x => x.ToJavaObject()).ToArray()));
        }

        public async Task<IDocumentReference> AddDocumentAsync(object data)
        {
            var documentReference = (DocumentReference) await _wrapped.Add(data.ToHashMap());
            return new DocumentReferenceWrapper(documentReference);
        }

        public async Task<IQuerySnapshot<T>> GetDocumentsAsync<T>()
        {
            return new QuerySnapshotWrapper<T>(await _wrapped.Get().AsAsync<QuerySnapshot>());
        }
    }
}