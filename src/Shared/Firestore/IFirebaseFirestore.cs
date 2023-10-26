using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Firestore
{
    /// <summary>
    /// Represents a Firestore Database and is the entry point for all Firestore operations.
    /// </summary>
    public interface IFirebaseFirestore : IDisposable
    {
        /// <summary>
        /// Creates and returns a new <c>IQuery</c> object that includes all documents in the database that are contained.
        /// </summary>
        /// <param name="collectionId">The collectionId of the collection or subcollection.</param>
        /// <returns></returns>
        IQuery GetCollectionGroup(string collectionId);
        
        /// <summary>
        /// Gets a <c>ICollectionReference</c> object referring to the collection at the specified path within the database.
        /// </summary>
        /// <param name="collectionPath">The slash-separated path of the collection for which to get a <c>ICollectionReference</c>.</param>
        /// <returns></returns>
        ICollectionReference GetCollection(string collectionPath);

        /// <summary>
        /// Gets a <c>IDocumentReference</c> object referring to the document at the specified path within the database.
        /// </summary>
        /// <param name="documentPath">The slash-separated path of the document for which to get a <c>IDocumentReference</c>.</param>
        /// <returns></returns>
        IDocumentReference GetDocument(string documentPath);

        /// <summary>
        /// Executes the given updateBlock and then attempts to commit the changes applied within an atomic transaction.
        /// </summary>
        /// <param name="updateFunc">The func to execute within the transaction context.</param>
        /// <typeparam name="TResult">The type of the result returned by updateFunc.</typeparam>
        /// <returns></returns>
        Task<TResult> RunTransactionAsync<TResult>(Func<ITransaction, TResult> updateFunc);

        /// <summary>
        /// Creates a write batch, used for performing multiple writes as a single atomic operation.
        /// </summary>
        /// <returns></returns>
        IWriteBatch CreateBatch();

        /// <summary>
        /// Modifies this FirebaseDatabase instance to communicate with the Cloud Firestore emulator.
        /// Note: Call this method before using the instance to do any database operations.
        /// </summary>
        /// <param name="host">The emulator host (for example, 10.0.2.2 on android and localhost on iOS)</param>
        /// <param name="port">The emulator port (for example, 8080)</param>
        void UseEmulator(string host, int port);

        /// <summary>
        /// Custom settings used to configure this <c>IFirebaseFirestore</c> object.
        /// </summary>
        FirestoreSettings Settings { get; set; }
    }
}