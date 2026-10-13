using Firebase.Firestore;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;
using Object = Java.Lang.Object;

namespace Plugin.Firebase.Firestore.Platforms.Android;

public class TransactionFunction<TResult> : Object, Transaction.IFunction
{
    private readonly Func<ITransaction, TResult> _func;

    public TransactionFunction(Func<ITransaction, TResult> func)
    {
        _func = func;
    }

    /// <summary>
    /// The managed exception thrown by the update function during the last <see cref="Apply"/>
    /// invocation, or <see langword="null"/> when the function completed or failed with a native Firestore
    /// error. Reset on each attempt so a failed attempt followed by a successful retry does not report a
    /// stale failure.
    /// </summary>
    internal Exception? UpdateException { get; private set; }

    /// <summary>
    /// The value returned by the update function during the last successful <see cref="Apply"/>
    /// invocation. The result never needs to be a Firestore value: it stays in .NET so any TResult
    /// works and nothing can fail after the commit.
    /// </summary>
    internal TResult? Result { get; private set; }

    public Object? Apply(Transaction transaction)
    {
        UpdateException = null;
        try {
            Result = _func(transaction.ToAbstract());
            return null;
        } catch(CrossPlatformFirebaseFirestoreException e) when(e.InnerException is FirebaseFirestoreException native) {
            // Hand the native exception back to the transaction runner: it retries only a
            // FirebaseFirestoreException with a retryable code. RunTransactionAsync wraps it again when
            // the transaction fails with it.
            throw native;
        } catch(FirebaseFirestoreException) {
            // A native exception thrown directly by the update function goes to the runner the same way.
            throw;
        } catch(Exception e) {
            UpdateException = e;
            throw;
        }
    }
}