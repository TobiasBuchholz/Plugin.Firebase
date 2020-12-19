using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Firestore
{
    public interface IFirebaseFirestore : IDisposable
    {
        ICollectionReference GetCollection(string collectionPath);
        IDocumentReference GetDocument(string documentPath);
        Task<TResult> RunTransactionAsync<TResult>(Func<ITransaction, TResult> updateFunc);
        IWriteBatch CreateBatch();
        
        FirestoreSettings Settings { get; set; }
    }
}