using System.Runtime.ExceptionServices;
using Android.Gms.Extensions;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore.Platforms.Android;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;
using FirebaseFirestore = Firebase.Firestore.FirebaseFirestore;

namespace Plugin.Firebase.Firestore;

public sealed class FirebaseFirestoreImplementation : DisposableBase, IFirebaseFirestore
{
    private FirebaseFirestore _firestore;

    public FirebaseFirestoreImplementation()
    {
        _firestore = FirebaseFirestore.Instance;
    }

    public IQuery GetCollectionGroup(string collectionId)
    {
        return new QueryWrapper(_firestore.CollectionGroup(collectionId));
    }

    public ICollectionReference GetCollection(string collectionPath)
    {
        return new CollectionReferenceWrapper(_firestore.Collection(collectionPath));
    }

    public IDocumentReference GetDocument(string documentPath)
    {
        return new DocumentReferenceWrapper(_firestore.Document(documentPath));
    }

    public async Task<TResult?> RunTransactionAsync<TResult>(Func<ITransaction, TResult> updateFunc)
    {
        var function = new TransactionFunction<TResult>(updateFunc);
        Java.Lang.Object? result;
        try {
            result = await _firestore.RunTransaction(function);
        } catch(Exception) when(function.UpdateException is { } updateException) {
            // the task faulted because the update function threw; surface the managed failure
            if(updateException is CrossPlatformFirebaseFirestoreException) {
                // rethrow without resetting the stack trace of the call that failed inside the update function
                ExceptionDispatchInfo.Throw(updateException);
            }
            throw new FirebaseException(updateException.Message, updateException);
        } catch(Exception e) when(FirestoreExceptionFactory.IsNativeFirestoreException(e)) {
            // includes a native failure that TransactionFunction handed back to the transaction runner
            throw FirestoreExceptionFactory.Create(e);
        }
        return (TResult?) result?.ToObject(typeof(TResult));
    }

    public IWriteBatch CreateBatch()
    {
        return _firestore.Batch().ToAbstract();
    }

    public Task WaitForPendingWritesAsync()
    {
        return FirestoreExceptionFactory.Wrap(() => _firestore.WaitForPendingWrites().AsAsync());
    }

    public Task DisableNetworkAsync()
    {
        return FirestoreExceptionFactory.Wrap(() => _firestore.DisableNetwork().AsAsync());
    }

    public Task EnableNetworkAsync()
    {
        return FirestoreExceptionFactory.Wrap(() => _firestore.EnableNetwork().AsAsync());
    }

    public Task ClearPersistenceAsync()
    {
        return FirestoreExceptionFactory.Wrap(() => _firestore.ClearPersistence().AsAsync());
    }

    public Task TerminateAsync()
    {
        return FirestoreExceptionFactory.Wrap(() => _firestore.Terminate().AsAsync());
    }

    public void Restart()
    {
        _firestore = FirebaseFirestore.Instance;
    }

    public void UseEmulator(string host, int port)
    {
        _firestore.UseEmulator(host, port);
    }

    public FirestoreSettings Settings {
        get => _firestore.FirestoreSettings.ToAbstract();
        set => _firestore.FirestoreSettings = value.ToNative();
    }
}