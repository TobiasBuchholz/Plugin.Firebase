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
        NSError? passedThroughError = null;
        NSObject? result;
        try {
            result = await _firestore.RunTransactionAsync(
                (Transaction transaction, ref NSError? error) => {
                    // Captured state is per attempt: a failed attempt followed by a successful retry
                    // must not report the earlier failure after committing.
                    exception = null;
                    passedThroughError = null;
                    try {
                        return updateFunc(transaction.ToAbstract())?.ToNSObject();
                    } catch(Exception e) {
                        // A failure already wrapped by this plugin, such as a failed transactional read,
                        // surfaces as-is instead of being wrapped twice.
                        exception = e as CrossPlatformFirebaseFirestoreException ?? new FirebaseException(e.Message, e);
                        // Only an error assigned to the ref parameter aborts the native transaction.
                        // A Firestore error domain goes through the SDK's retry rules, so a retryable
                        // transaction read failure inside the update function is retried like it is
                        // on Android. Any other domain marks the transaction permanently failed, so
                        // the writes queued before the failure can neither be retried nor committed.
                        passedThroughError = FindFirestoreError(e);
                        error = passedThroughError ?? NSError.FromDomain(
                            TransactionUpdateErrorDomain,
                            -1,
                            new NSDictionary<NSString, NSObject>(NSError.LocalizedDescriptionKey, new NSString(e.Message)));
                        return null;
                    }
                }
            );
        } catch(Exception) when(exception != null && passedThroughError == null) {
            // the task faulted with the native error assigned above; surface the original managed failure instead
            if(exception is CrossPlatformFirebaseFirestoreException) {
                // rethrow without resetting the stack trace of the call that failed inside the update function
                ExceptionDispatchInfo.Throw(exception);
            }
            throw exception;
        } catch(Exception e) when(FirestoreExceptionFactory.IsNativeFirestoreException(e)) {
            throw FirestoreExceptionFactory.Create(e);
        }
        return (TResult?) result?.ToObject(typeof(TResult));
    }

    private static NSError? FindFirestoreError(Exception exception)
    {
        for(var e = exception; e != null; e = e.InnerException) {
            if(e is NSErrorException { Error: { } error }
                && error.Domain.ToString() == FBFirestore.ErrorDomain.ToString()) {
                return error;
            }
        }
        return null;
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