using System;
using System.Threading.Tasks;
using Firebase.CloudFirestore;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.iOS.Extensions;

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

        public IQuery WhereGreaterThan(string field, object value)
        {
            return _wrapped.WhereGreaterThan(field, value).ToAbstract();
        }

        public IQuery WhereLessThan(string field, object value)
        {
            return _wrapped.WhereLessThan(field, value).ToAbstract();
        }

        public IQuery WhereGreaterThanOrEqualsTo(string field, object value)
        {
            return _wrapped.WhereGreaterThanOrEqualsTo(field, value).ToAbstract();
        }

        public IQuery WhereLessThanOrEqualsTo(string field, object value)
        {
            return _wrapped.WhereLessThanOrEqualsTo(field, value).ToAbstract();
        }

        public IQuery OrderBy(string field)
        {
            return _wrapped.OrderedBy(field).ToAbstract();
        }

        public IQuery StartingAt(object[] fieldValues)
        {
            return _wrapped.StartingAt(fieldValues).ToAbstract();
        }

        public IQuery EndingAt(object[] fieldValues)
        {
            return _wrapped.EndingAt(fieldValues).ToAbstract();
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
                    tcs.SetException(new FirebaseException(error?.LocalizedDescription));
                }
            });
            return tcs.Task;
        }

        public async Task<IQuerySnapshot<T>> GetDocumentsAsync<T>()
        {
            return new QuerySnapshotWrapper<T>(await _wrapped.GetDocumentsAsync());
        }
    }
}