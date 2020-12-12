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
            return new QueryWrapper(_wrapped.WhereEqualsTo(field, value));
        }

        public IQuery WhereGreaterThan(string field, object value)
        {
            return new QueryWrapper(_wrapped.WhereGreaterThan(field, value));
        }

        public IQuery WhereLessThan(string field, object value)
        {
            return new QueryWrapper(_wrapped.WhereLessThan(field, value));
        }

        public IQuery WhereGreaterThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_wrapped.WhereGreaterThanOrEqualsTo(field, value));
        }

        public IQuery WhereLessThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_wrapped.WhereLessThanOrEqualsTo(field, value));
        }

        public IQuery OrderBy(string field)
        {
            return new QueryWrapper(_wrapped.OrderedBy(field));
        }

        public IQuery StartingAt(object[] fieldValues)
        {
            return new QueryWrapper(_wrapped.StartingAt(fieldValues));
        }

        public IQuery EndingAt(object[] fieldValues)
        {
            return new QueryWrapper(_wrapped.EndingAt(fieldValues));
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