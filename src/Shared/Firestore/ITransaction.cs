using System.Collections.Generic;

namespace Plugin.Firebase.Firestore
{
    public interface ITransaction
    {
        IDocumentSnapshot<T> GetDocument<T>(IDocumentReference document);
        ITransaction SetData(IDocumentReference document, object data, SetOptions options = null);
        ITransaction SetData(IDocumentReference document, Dictionary<object, object> data, SetOptions options = null);
        ITransaction SetData(IDocumentReference document, params (object, object)[] data);
        ITransaction SetData(IDocumentReference document, SetOptions options, params (object, object)[] data);
        ITransaction UpdateData(IDocumentReference document, Dictionary<object, object> data);
        ITransaction UpdateData(IDocumentReference document, params (string, object)[] data);
        ITransaction DeleteDocument(IDocumentReference document);
    }
}