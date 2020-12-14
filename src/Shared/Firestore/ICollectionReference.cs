using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Firestore
{
    public interface ICollectionReference
    {
        IDisposable AddSnapshotListener<T>(Action<IQuerySnapshot<T>> onChanged, Action<Exception> onError = null, bool includeMetaDataChanges = false);
        IDocumentReference GetDocument(string documentPath);
        IDocumentReference CreateDocument();
        IQuery WhereEqualsTo(string field, object value);
        IQuery WhereGreaterThan(string field, object value);
        IQuery WhereLessThan(string field, object value);
        IQuery WhereGreaterThanOrEqualsTo(string field, object value);
        IQuery WhereLessThanOrEqualsTo(string field, object value);
        IQuery OrderBy(string field);
        IQuery StartingAt(object[] fieldValues);
        IQuery EndingAt(object[] fieldValues);
        IQuery LimitedTo(int limit);
        IQuery LimitedToLast(int limit);
        Task<IDocumentReference> AddDocumentAsync(object data);
        Task<IQuerySnapshot<T>> GetDocumentsAsync<T>();
    }
}