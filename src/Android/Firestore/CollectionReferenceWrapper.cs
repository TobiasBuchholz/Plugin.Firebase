using System;
using System.Collections.Generic;
using System.Linq;
using Plugin.Firebase.Extensions;
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
        private readonly CollectionReference _reference;
        
        public CollectionReferenceWrapper(CollectionReference reference)
        {
            _reference = reference;
        }
        
        public IDisposable AddSnapshotListener<T>(Action<IQuerySnapshot<T>> onChanged, Action<Exception> onError = null, bool includeMetaDataChanges = false)
        {
            var registration = _reference
                .AddSnapshotListener(includeMetaDataChanges ? MetadataChanges.Include : MetadataChanges.Exclude, new EventListener(
                    x => onChanged(new QuerySnapshotWrapper<T>(x.JavaCast<QuerySnapshot>())), 
                    e => onError?.Invoke(new FirebaseException(e.LocalizedMessage))));
            return new DisposableWithAction(registration.Remove);
        }

        public IDocumentReference GetDocument(string documentPath)
        {
            return new DocumentReferenceWrapper(_reference.Document(documentPath));
        }

        public IDocumentReference CreateDocument()
        {
            return new DocumentReferenceWrapper(_reference.Document());
        }

        public IQuery WhereEqualsTo(string field, object value)
        {
            return new QueryWrapper(_reference.WhereEqualTo(field, value.ToJavaObject()));
        }

        public IQuery WhereGreaterThan(string field, object value)
        {
            return new QueryWrapper(_reference.WhereGreaterThan(field, value.ToJavaObject()));
        }

        public IQuery WhereLessThan(string field, object value)
        {
            return new QueryWrapper(_reference.WhereLessThan(field, value.ToJavaObject()));
        }

        public IQuery WhereGreaterThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_reference.WhereGreaterThanOrEqualTo(field, value.ToJavaObject()));
        }

        public IQuery WhereLessThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_reference.WhereLessThanOrEqualTo(field, value.ToJavaObject()));
        }

        public IQuery OrderBy(string field)
        {
            return new QueryWrapper(_reference.OrderBy(field));
        }

        public IQuery StartingAt(object[] fieldValues)
        {
            return new QueryWrapper(_reference.StartAt(fieldValues.Select(x => x.ToJavaObject()).ToArray()));
        }

        public IQuery EndingAt(object[] fieldValues)
        {
            return new QueryWrapper(_reference.EndAt(fieldValues.Select(x => x.ToJavaObject()).ToArray()));
        }

        public async Task<IDocumentReference> AddDocumentAsync(object data)
        {
            var documentReference = (DocumentReference) await _reference.Add(data.ToHashMap());
            return new DocumentReferenceWrapper(documentReference);
        }

        public async Task<IQuerySnapshot<T>> GetDocumentsAsync<T>()
        {
            return new QuerySnapshotWrapper<T>(await _reference.Get().ToTask<QuerySnapshot>());
        }
    }
}