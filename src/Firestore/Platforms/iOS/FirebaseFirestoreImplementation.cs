using Firebase.CloudFirestore;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore.Platforms.iOS;
using Plugin.Firebase.Firestore.Platforms.iOS.Extensions;
using FBFirestore = Firebase.CloudFirestore.Firestore;

namespace Plugin.Firebase.Firestore;

/// <summary>
/// iOS implementation of <see cref="IFirebaseFirestore"/> that wraps the native Firebase Firestore SDK.
/// </summary>
public sealed class FirebaseFirestoreImplementation : DisposableBase, IFirebaseFirestore
{
    private static readonly NSString TransactionUpdateErrorDomain = new NSString("Plugin.Firebase.Firestore");

    private FBFirestore _firestore;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirebaseFirestoreImplementation"/> class.
    /// </summary>
    public FirebaseFirestoreImplementation()
    {
        _firestore = FBFirestore.SharedInstance;
    }

    /// <inheritdoc/>
    public IQuery GetCollectionGroup(string collectionId)
    {
        return new QueryWrapper(_firestore.GetCollectionGroup(collectionId));
    }

    /// <inheritdoc/>
    public ICollectionReference GetCollection(string collectionPath)
    {
        return new CollectionReferenceWrapper(_firestore.GetCollection(collectionPath));
    }

    /// <inheritdoc/>
    public IDocumentReference GetDocument(string documentPath)
    {
        return new DocumentReferenceWrapper(_firestore.GetDocument(documentPath));
    }

    /// <inheritdoc/>
    public async Task<TResult?> RunTransactionAsync<TResult>(Func<ITransaction, TResult> updateFunc)
    {
        FirebaseException? exception = null;
        NSObject? result;
        try {
            result = await _firestore.RunTransactionAsync(
                (Transaction transaction, ref NSError? error) => {
                    try {
                        if(error == null) {
                            return updateFunc(transaction.ToAbstract())?.ToNSObject();
                        } else {
                            exception = new FirebaseException(error.LocalizedDescription);
                        }
                    } catch(Exception e) {
                        exception = new FirebaseException(e.Message, e);
                        // Only an error assigned to the ref parameter aborts the native transaction. A domain
                        // other than the Firestore error domain also marks the transaction permanently failed,
                        // so the writes queued before the failure can neither be retried nor committed.
                        error = NSError.FromDomain(
                            TransactionUpdateErrorDomain,
                            -1,
                            new NSDictionary<NSString, NSObject>(NSError.LocalizedDescriptionKey, new NSString(e.Message)));
                    }
                    return null;
                }
            );
        } catch(Exception) when(exception != null) {
            // the task faulted with the native error assigned above; surface the original managed failure instead
            throw exception;
        }
        return exception is null ? (TResult?) result?.ToObject(typeof(TResult)) : throw exception;
    }

    /// <inheritdoc/>
    public IWriteBatch CreateBatch()
    {
        return _firestore.CreateBatch().ToAbstract();
    }

    /// <inheritdoc/>
    public Task WaitForPendingWritesAsync()
    {
        return _firestore.WaitForPendingWritesAsync();
    }

    /// <inheritdoc/>
    public Task DisableNetworkAsync()
    {
        return _firestore.DisableNetworkAsync();
    }

    /// <inheritdoc/>
    public Task EnableNetworkAsync()
    {
        return _firestore.EnableNetworkAsync();
    }

    /// <inheritdoc/>
    public Task ClearPersistenceAsync()
    {
        return _firestore.ClearPersistenceAsync();
    }

    /// <inheritdoc/>
    public Task TerminateAsync()
    {
        return _firestore.TerminateAsync();
    }

    /// <inheritdoc/>
    public void Restart()
    {
        _firestore = FBFirestore.SharedInstance;
    }

    /// <inheritdoc/>
    public void UseEmulator(string host, int port)
    {
        _firestore.UseEmulatorWithHost(host, port);
        Settings = new FirestoreSettings(Settings.Host);
    }

    /// <inheritdoc/>
    public FirestoreSettings Settings {
        get => _firestore.Settings.ToAbstract();
        set => _firestore.Settings = value.ToNative();
    }
}