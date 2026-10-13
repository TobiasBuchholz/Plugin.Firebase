using System.Runtime.ExceptionServices;
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
        TResult? result = default;
        try {
            await _firestore.RunTransactionAsync(
                (Transaction transaction, ref NSError? error) => {
                    // Captured state is per attempt: when a retry fails after its update function completed,
                    // for example at commit, the failure of an earlier attempt must not be surfaced instead.
                    exception = null;
                    try {
                        // The result never needs to be a Firestore value: keep it in .NET so any
                        // TResult works and nothing can fail after the commit.
                        result = updateFunc(transaction.ToAbstract());
                        return null;
                    } catch(Exception e) {
                        // A failure already wrapped by this plugin, such as a failed transactional read,
                        // surfaces as-is instead of being wrapped twice.
                        exception = e as CrossPlatformFirebaseFirestoreException ?? new FirebaseException(e.Message, e);
                        // Only an error assigned to the ref parameter aborts the native transaction. Native
                        // Firestore applies its retry rules only to an error in the Firestore domain, so when
                        // updateFunc lets a failed transactional read propagate, the SDK's own NSError is handed
                        // back and the transaction is retried if its code is retryable, as on Android. Any other
                        // failure, including one that wraps a failed read, is reported in the plugin's domain,
                        // which marks the transaction permanently failed, so the writes queued before the
                        // failure can neither be retried nor committed.
                        error = FirestoreExceptionFactory.GetFirestoreError(e) ?? NSError.FromDomain(
                            TransactionUpdateErrorDomain,
                            -1,
                            new NSDictionary<NSString, NSObject>(NSError.LocalizedDescriptionKey, new NSString(e.Message)));
                        return null;
                    }
                }
            );
        } catch(Exception) when(exception != null) {
            // the task faulted with the error assigned in the last attempt; surface its managed failure instead
            if(exception is CrossPlatformFirebaseFirestoreException) {
                // rethrow without resetting the stack trace of the call that failed inside the update function
                ExceptionDispatchInfo.Throw(exception);
            }
            throw exception;
        } catch(Exception e) when(FirestoreExceptionFactory.IsNativeFirestoreException(e)) {
            throw FirestoreExceptionFactory.Create(e);
        }
        return result;
    }

    /// <inheritdoc/>
    public IWriteBatch CreateBatch()
    {
        return _firestore.CreateBatch().ToAbstract();
    }

    /// <inheritdoc/>
    public Task WaitForPendingWritesAsync()
    {
        return FirestoreExceptionFactory.Wrap(() => _firestore.WaitForPendingWritesAsync());
    }

    /// <inheritdoc/>
    public Task DisableNetworkAsync()
    {
        return FirestoreExceptionFactory.Wrap(() => _firestore.DisableNetworkAsync());
    }

    /// <inheritdoc/>
    public Task EnableNetworkAsync()
    {
        return FirestoreExceptionFactory.Wrap(() => _firestore.EnableNetworkAsync());
    }

    /// <inheritdoc/>
    public Task ClearPersistenceAsync()
    {
        return FirestoreExceptionFactory.Wrap(() => _firestore.ClearPersistenceAsync());
    }

    /// <inheritdoc/>
    public Task TerminateAsync()
    {
        return FirestoreExceptionFactory.Wrap(() => _firestore.TerminateAsync());
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