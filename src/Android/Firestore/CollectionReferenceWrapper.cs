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
            return _wrapped.WhereEqualTo(field, value.ToJavaObject()).ToAbstract();
        }

        public IQuery WhereGreaterThan(string field, object value)
        {
            return _wrapped.WhereGreaterThan(field, value.ToJavaObject()).ToAbstract();
        }

        public IQuery WhereLessThan(string field, object value)
        {
            return _wrapped.WhereLessThan(field, value.ToJavaObject()).ToAbstract();
        }

        public IQuery WhereGreaterThanOrEqualsTo(string field, object value)
        {
            return _wrapped.WhereGreaterThanOrEqualTo(field, value.ToJavaObject()).ToAbstract();
        }

        public IQuery WhereLessThanOrEqualsTo(string field, object value)
        {
            return _wrapped.WhereLessThanOrEqualTo(field, value.ToJavaObject()).ToAbstract();
        }

        public IQuery OrderBy(string field, bool descending = false)
        {
            return _wrapped.OrderBy(field, descending ? Query.Direction.Descending : Query.Direction.Ascending).ToAbstract();
        }

        public IQuery StartingAt(params object[] fieldValues)
        {
            return _wrapped.StartAt(fieldValues.Select(x => x.ToJavaObject()).ToArray()).ToAbstract();
        }

        public IQuery StartingAt(IDocumentSnapshot snapshot)
        {
            return _wrapped.StartAt(snapshot.ToNative()).ToAbstract();
        }
        
        public IQuery StartingAfter(params object[] fieldValues)
        {
            return _wrapped.StartAfter(fieldValues.Select(x => x.ToJavaObject()).ToArray()).ToAbstract();
        }

        public IQuery StartingAfter(IDocumentSnapshot snapshot)
        {
            return _wrapped.StartAfter(snapshot.ToNative()).ToAbstract();
        }

        public IQuery EndingAt(params object[] fieldValues)
        {
            return _wrapped.EndAt(fieldValues.Select(x => x.ToJavaObject()).ToArray()).ToAbstract();
        }

        public IQuery EndingAt(IDocumentSnapshot snapshot)
        {
            return _wrapped.EndAt(snapshot.ToNative()).ToAbstract();
        }

        public IQuery EndingBefore(params object[] fieldValues)
        {
            return _wrapped.EndBefore(fieldValues.Select(x => x.ToJavaObject()).ToArray()).ToAbstract();
        }

        public IQuery EndingBefore(IDocumentSnapshot snapshot)
        {
            return _wrapped.EndBefore(snapshot.ToNative()).ToAbstract();
        }

        public IQuery LimitedTo(int limit)
        {
            return _wrapped.Limit(limit).ToAbstract();
        }

        public IQuery LimitedToLast(int limit)
        {
            return _wrapped.LimitToLast(limit).ToAbstract();
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