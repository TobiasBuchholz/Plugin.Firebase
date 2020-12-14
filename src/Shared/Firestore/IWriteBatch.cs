using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.Firestore
{
    public interface IWriteBatch
    {
        IWriteBatch SetData(IDocumentReference document, object data, SetOptions options = null);
        IWriteBatch SetData(IDocumentReference document, Dictionary<object, object> data, SetOptions options = null);
        IWriteBatch SetData(IDocumentReference document, params (object, object)[] data);
        IWriteBatch SetData(IDocumentReference document, SetOptions options, params (object, object)[] data);
        IWriteBatch UpdateData(IDocumentReference document, Dictionary<object, object> data);
        IWriteBatch UpdateData(IDocumentReference document, params (string, object)[] data);
        IWriteBatch DeleteDocument(IDocumentReference document);
        Task CommitAsync();
    }
}