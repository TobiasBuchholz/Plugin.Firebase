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
        /// Waits until all currently pending writes for the active user have been acknowledged by the backend.
        ///
        /// The returned Task completes immediately if there are no outstanding writes. Otherwise, the Task waits for all previously issued
        /// writes (including those written in a previous app session), but it does not wait for writes that were added after the method is called.
        /// If you wish to wait for additional writes, you have to call <c>WaitForPendingWritesAsync()</c> again.
        ///
        /// Any outstanding <c>WaitForPendingWritesAsync()</c> Tasks are cancelled during user changes.
        /// </summary>
        /// <returns></returns>
        Task WaitForPendingWritesAsync();

        /// <summary>
        /// Disables network access for this instance. While the network is disabled, any snapshot listeners or get() calls will
        /// return results from cache, and any write operations will be queued until network usage is re-enabled via a call to enableNetwork
        /// </summary>
        /// <returns></returns>
        Task DisableNetworkAsync();

        /// <summary>
        /// Re-enables network usage for this instance after a prior call to <c>DisableNetworkAsync</c>.
        /// </summary>
        /// <returns></returns>
        Task EnableNetworkAsync();

        /// <summary>
        /// Clears the persistent storage, including pending writes and cached documents.
        ///
        /// Must be called while the <c>IFirebaseFirestore</c> instance is not started (after the app is shutdown or when the app is first initialized).
        /// On startup, this method must be called before other methods (other than setting <c>FirestoreSettings</c>). If the <c>IFirebaseFirestore</c>
        /// instance is still running, the Task will fail with an error code of FAILED_PRECONDITION.
        ///
        /// Note: <c>ClearPersistenceAsync()</c> is primarily intended to help write reliable tests that use Cloud Firestore. It uses an efficient
        /// mechanism for dropping existing data but does not attempt to securely overwrite or otherwise make cached data unrecoverable. For
        /// applications that are sensitive to the disclosure of cached data in between user sessions, we strongly recommend not enabling
        /// persistence at all.
        /// </summary>
        /// <returns></returns>
        Task ClearPersistenceAsync();

        /// <summary>
        /// Terminates this <c>IFirebaseFirestore</c> instance.
        /// 
        /// After calling <c>TerminateAsync()</c> only the <c>ClearPersistenceAsync()</c> method may be used. Any other method will throw
        /// an IllegalStateException.
        /// 
        /// To restart after termination, simply call the <c>Restart()</c> method.
        ///
        /// <c>TerminateAsync()</c> does not cancel any pending writes and any tasks that are awaiting a response from the server will not be resolved.
        /// The next time you start this instance, it will resume attempting to send these writes to the server.
        /// 
        /// Note: Under normal circumstances, calling <c>TerminateAsync()</c> is not required. This method is useful only when you want to force this instance
        /// to release all of its resources or in combination with <c>ClearPersistenceAsync()</c> to ensure that all local state is destroyed between test runs.
        /// </summary>
        /// <returns></returns>
        Task TerminateAsync();

        /// <summary>
        /// Should be used after <c>TerminateAsync()</c> was called to restart the <c>IFirebaseFirestore</c> instance.
        /// </summary>
        void Restart();

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
