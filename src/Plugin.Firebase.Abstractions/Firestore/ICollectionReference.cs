using System.Threading.Tasks;

namespace Plugin.Firebase.Abstractions.Firestore
{
    public interface ICollectionReference
    {
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
        Task<IDocumentReference> AddDocumentAsync(object data);
    }
}