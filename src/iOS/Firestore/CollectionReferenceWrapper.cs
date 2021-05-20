using System;
using System.Threading.Tasks;
using Firebase.CloudFirestore;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.iOS.Extensions;
using FieldPath = Plugin.Firebase.Firestore.FieldPath;

namespace Plugin.Firebase.iOS.Firestore
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
            var registration = _wrapped.AddSnapshotListener(includeMetaDataChanges, (snapshot, error) => {
                if(error == null) {
                    onChanged(new QuerySnapshotWrapper<T>(snapshot));                    
                } else {
                    onError?.Invoke(new FirebaseException(error.LocalizedDescription));
                }
            });
            return new DisposableWithAction(registration.Remove);
        }
        
        public IDocumentReference GetDocument(string documentPath)
        {
            return new DocumentReferenceWrapper(_wrapped.GetDocument(documentPath));
        }

        public IDocumentReference CreateDocument()
        {
            return new DocumentReferenceWrapper(_wrapped.CreateDocument());
        }

        public IQuery WhereEqualsTo(string field, object value)
        {
            return _wrapped.WhereEqualsTo(field, value).ToAbstract();
        }

        public IQuery WhereEqualsTo(FieldPath path, object value)
        {
            return _wrapped.WhereEqualsTo(path.ToNative(), value).ToAbstract();
        }

        public IQuery WhereGreaterThan(string field, object value)
        {
            return _wrapped.WhereGreaterThan(field, value).ToAbstract();
        }

        public IQuery WhereGreaterThan(FieldPath path, object value)
        {
            return _wrapped.WhereGreaterThan(path.ToNative(), value).ToAbstract();
        }

        public IQuery WhereLessThan(string field, object value)
        {
            return _wrapped.WhereLessThan(field, value).ToAbstract();
        }

        public IQuery WhereLessThan(FieldPath path, object value)
        {
            return _wrapped.WhereLessThan(path.ToNative(), value).ToAbstract();
        }

        public IQuery WhereGreaterThanOrEqualsTo(string field, object value)
        {
            return _wrapped.WhereGreaterThanOrEqualsTo(field, value).ToAbstract();
        }

        public IQuery WhereGreaterThanOrEqualsTo(FieldPath path, object value)
        {
            return _wrapped.WhereGreaterThanOrEqualsTo(path.ToNative(), value).ToAbstract();
        }

        public IQuery WhereLessThanOrEqualsTo(string field, object value)
        {
            return _wrapped.WhereLessThanOrEqualsTo(field, value).ToAbstract();
        }

        public IQuery WhereLessThanOrEqualsTo(FieldPath path, object value)
        {
            return _wrapped.WhereLessThanOrEqualsTo(path.ToNative(), value).ToAbstract();
        }

        public IQuery WhereArrayContains(string field, object value)
        {
            return _wrapped.WhereArrayContains(field, value).ToAbstract();
        }

        public IQuery WhereArrayContains(FieldPath path, object value)
        {
            return _wrapped.WhereArrayContains(path.ToNative(), value).ToAbstract();
        }

        public IQuery WhereArrayContainsAny(string field, object[] values)
        {
            return _wrapped.WhereArrayContainsAny(field, values).ToAbstract();
        }

        public IQuery WhereArrayContainsAny(FieldPath path, object[] values)
        {
            return _wrapped.WhereArrayContainsAny(path.ToNative(), values).ToAbstract();
        }

        public IQuery WhereFieldIn(string field, object[] values)
        {
            return _wrapped.WhereFieldIn(field, values).ToAbstract();
        }

        public IQuery WhereFieldIn(FieldPath path, object[] values)
        {
            return _wrapped.WhereFieldIn(path.ToNative(), values).ToAbstract();
        }

        public IQuery OrderBy(string field, bool descending = false)
        {
            return _wrapped.OrderedBy(field, descending).ToAbstract();
        }

        public IQuery OrderBy(FieldPath path, bool @descending = false)
        {
            return _wrapped.OrderedBy(path.ToNative(), descending).ToAbstract();
        }

        public IQuery StartingAt(params object[] fieldValues)
        {
            return _wrapped.StartingAt(fieldValues).ToAbstract();
        }

        public IQuery StartingAt(IDocumentSnapshot snapshot)
        {
            return _wrapped.StartingAt(snapshot.ToNative()).ToAbstract();
        }

        public IQuery StartingAfter(params object[] fieldValues)
        {
            return _wrapped.StartingAfter(fieldValues).ToAbstract();
        }

        public IQuery StartingAfter(IDocumentSnapshot snapshot)
        {
            return _wrapped.StartingAfter(snapshot.ToNative()).ToAbstract();
        }

        public IQuery EndingAt(params object[] fieldValues)
        {
            return _wrapped.EndingAt(fieldValues).ToAbstract();
        }
        
        public IQuery EndingAt(IDocumentSnapshot snapshot)
        {
            return _wrapped.EndingAt(snapshot.ToNative()).ToAbstract();
        }

        public IQuery EndingBefore(params object[] fieldValues)
        {
            return _wrapped.EndingBefore(fieldValues).ToAbstract();
        }

        public IQuery EndingBefore(IDocumentSnapshot snapshot)
        {
            return _wrapped.EndingBefore(snapshot.ToNative()).ToAbstract();
        }

        public IQuery LimitedTo(int limit)
        {
            return _wrapped.LimitedTo(limit).ToAbstract();
        }

        public IQuery LimitedToLast(int limit)
        {
            return _wrapped.LimitedToLast(limit).ToAbstract();
        }
        
        public Task<IDocumentReference> AddDocumentAsync(object data)
        {
            var tcs = new TaskCompletionSource<IDocumentReference>();
            DocumentReference documentReference = null;
            documentReference = _wrapped.AddDocument(data.ToDictionary(), error => {
                if(error == null) {
                    tcs.SetResult(new DocumentReferenceWrapper(documentReference));
                } else {
                    tcs.SetException(new FirebaseException(error.LocalizedDescription));
                }
            });
            return tcs.Task;
        }

        public Task<IQuerySnapshot<T>> GetDocumentsAsync<T>(Source source = Source.Default)
        {
            var tcs = new TaskCompletionSource<IQuerySnapshot<T>>();
            _wrapped.GetDocuments(source.ToNative(), (snapshot, error) => {
                if(error == null) {
                    tcs.SetResult(new QuerySnapshotWrapper<T>(snapshot));
                } else {
                    tcs.SetException(new FirebaseException(error.LocalizedDescription));
                }
            });
            return tcs.Task;
        }
    }
}