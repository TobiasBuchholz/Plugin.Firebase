using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Android.Runtime;
using Firebase.Firestore;
using Plugin.Firebase.Common;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Android.Extensions;

namespace Plugin.Firebase.Android.Firestore
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
            return new QueryWrapper(_query.WhereEqualTo(field, value.ToJavaObject()));
        }

        public IQuery WhereGreaterThan(string field, object value)
        {
            return new QueryWrapper(_query.WhereGreaterThan(field, value.ToJavaObject()));
        }

        public IQuery WhereLessThan(string field, object value)
        {
            return new QueryWrapper(_query.WhereLessThan(field, value.ToJavaObject()));
        }

        public IQuery WhereGreaterThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_query.WhereGreaterThanOrEqualTo(field, value.ToJavaObject()));
        }

        public IQuery WhereLessThanOrEqualsTo(string field, object value)
        {
            return new QueryWrapper(_query.WhereLessThanOrEqualTo(field, value.ToJavaObject()));
        }

        public IQuery OrderBy(string field)
        {
            return new QueryWrapper(_query.OrderBy(field));
        }

        public IQuery StartingAt(object[] fieldValues)
        {
            return new QueryWrapper(_query.StartAt(fieldValues.Select(x => x.ToJavaObject()).ToArray()));
        }

        public IQuery EndingAt(object[] fieldValues)
        {
            return new QueryWrapper(_query.EndAt(fieldValues.Select(x => x.ToJavaObject()).ToArray()));
        }

        public IQuery LimitedTo(int limit)
        {
            return new QueryWrapper(_query.Limit(limit));
        }

        public async Task<IQuerySnapshot<T>> GetDocumentsAsync<T>()
        {
            var querySnapshot = (QuerySnapshot) await _query.Get();
            return new QuerySnapshotWrapper<T>(querySnapshot);
        }

        public IDisposable AddSnapshotListener<T>(Action<IQuerySnapshot<T>> onChanged, Action<Exception> onError = null, bool includeMetaDataChanges = false)
        {
            var registration = _query
                .AddSnapshotListener(includeMetaDataChanges ? MetadataChanges.Include : MetadataChanges.Exclude, new EventListener(
                    x => onChanged(new QuerySnapshotWrapper<T>(x.JavaCast<QuerySnapshot>())), 
                    e => onError?.Invoke(new FirebaseException(e.LocalizedMessage))));
            return new DisposableWithAction(registration.Remove);
        }
    }
}