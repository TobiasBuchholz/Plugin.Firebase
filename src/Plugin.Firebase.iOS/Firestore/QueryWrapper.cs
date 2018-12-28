using System;
using System.Threading.Tasks;
using Plugin.Firebase.Abstractions.Common;
using Plugin.Firebase.Abstractions.Firestore;
using Query = Firebase.CloudFirestore.Query;

namespace Plugin.Firebase.iOS.Firestore
{
    public sealed class QueryWrapper : IQuery
    {
        private readonly Query _query;

        public QueryWrapper(Query query)
        {
            _query = query;
        }

        public IQuery WhereEqualsTo(string field, object value)
        {
            return new QueryWrapper(_query.WhereEqualsTo(field, value));
        }

        public IQuery WhereGreaterThan(string field, object value)
        {
            return new QueryWrapper(_query.WhereGreaterThan(field, value));
        }

        public IQuery WhereLessThan(string field, object value)
        {
            return new QueryWrapper(_query.WhereLessThan(field, value));
        }

        public IQuery WhereGreaterThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_query.WhereGreaterThanOrEqualsTo(field, value));
        }

        public IQuery WhereLessThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_query.WhereLessThanOrEqualsTo(field, value));
        }

        public IQuery OrderBy(string field)
        {
            return new QueryWrapper(_query.OrderedBy(field));
        }

        public IQuery StartingAt(object[] fieldValues)
        {
            return new QueryWrapper(_query.StartingAt(fieldValues));
        }

        public IQuery EndingAt(object[] fieldValues)
        {
            return new QueryWrapper(_query.EndingAt(fieldValues));
        }
        
        public async Task<IQuerySnapshot<T>> GetDocumentsAsync<T>()
        {
            var querySnapshot = await _query.GetDocumentsAsync();
            return new QuerySnapshotWrapper<T>(querySnapshot);
        }

        public IDisposable AddSnapshotListener<T>(
            Action<IQuerySnapshot<T>> onChanged,
            Action<Exception> onError = null,
            bool includeMetaDataChanges = false)
        {
            var registration = _query.AddSnapshotListener(includeMetaDataChanges, (snapshot, error) => {
                if(error == null) {
                    onChanged(new QuerySnapshotWrapper<T>(snapshot));                    
                } else {
                    onError?.Invoke(new FirebaseException(error.LocalizedDescription));
                }
            });
            return new DisposableWithAction(registration.Remove);
        }
    }
}