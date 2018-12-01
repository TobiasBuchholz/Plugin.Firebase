using System;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Android.Runtime;
using Firebase.Firestore;
using Plugin.Firebase.Abstractions.Common;
using Plugin.Firebase.Abstractions.Firestore;
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

        public async Task<IQuerySnapshot<T>> GetDocumentsAsync<T>()
        {
            var querySnapshot = (QuerySnapshot) await _query.Get();
            return new QuerySnapshotWrapper<T>(querySnapshot);
        }

        public IDisposable AddSnapshotListener<T>(Action<IQuerySnapshot<T>> onChanged, Action<Exception> onError = null, bool includeMetaDataChanges = false)
        {
            var registration = _query
                .AddSnapshotListener(GetQueryListenOptions(includeMetaDataChanges), new EventListener(
                    x => onChanged(new QuerySnapshotWrapper<T>(x.JavaCast<QuerySnapshot>())), 
                    e => onError?.Invoke(new FirebaseException(e.LocalizedMessage))));
            return new DisposableWithAction(registration.Remove);
        }

        private static QueryListenOptions GetQueryListenOptions(bool includeMetaDataChanges)
        {
            var options = new QueryListenOptions();
            if(includeMetaDataChanges) {
                options.IncludeQueryMetadataChanges();
            }
            return options;
        }
    }
}