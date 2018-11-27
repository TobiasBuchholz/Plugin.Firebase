using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Abstractions.Firestore
{
    public interface IQuery
    {
        IQuery WhereEqualsTo(string field, object value);
        IQuery WhereGreaterThan(string field, object value);
        IQuery WhereLessThan(string field, object value);
        IQuery WhereGreaterThanOrEqualsTo(string field, object value);
        IQuery WhereLessThanOrEqualsTo(string field, object value);

        Task<IQuerySnapshot<T>> GetDocumentsAsync<T>();
        IDisposable AddSnapshotListener<T>(Action<IQuerySnapshot<T>> onChanged, Action<Exception> onError = null, bool includeMetaDataChanges = false);
    }
}