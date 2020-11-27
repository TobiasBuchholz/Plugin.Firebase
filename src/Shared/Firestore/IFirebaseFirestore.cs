using System;

namespace Plugin.Firebase.Firestore
{
    public interface IFirebaseFirestore : IDisposable
    {
        ICollectionReference GetCollection(string collectionPath);
        IDocumentReference GetDocument(string documentPath);
    }
}