using Firebase.Firestore;
using Plugin.Firebase.Core.Exceptions;
using Plugin.Firebase.Firestore.Platforms.Android.Extensions;
using Object = Java.Lang.Object;

namespace Plugin.Firebase.Firestore.Platforms.Android;

/// <summary>
/// Runs the update function of <see cref="FirebaseFirestoreImplementation.RunTransactionAsync{TResult}"/> for each attempt
/// of the native transaction and keeps its result and failure in .NET.
/// </summary>
internal class TransactionFunction<TResult> : Object, Transaction.IFunction
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
    /// The <see cref="ArgumentException"/> that aborted the last <see cref="Apply"/> invocation because the update
    /// function returned a task, or <see langword="null"/>. RunTransactionAsync surfaces it as-is. Reset on each attempt.
    /// </summary>
    internal ArgumentException? ResultException { get; private set; }

    /// <summary>
    /// The value returned by the update function during the last <see cref="Apply"/> invocation, or
    /// <see langword="default"/> when that attempt failed. When the transaction succeeds, this is the result of the
    /// attempt that committed. The result never needs to be a Firestore value: it stays in .NET so any TResult
    /// works and nothing can fail after the commit. Reset on each attempt.
    /// </summary>
    internal TResult? Result { get; private set; }

    public Object? Apply(Transaction transaction)
    {
        UpdateException = null;
        ResultException = null;
        Result = default;
        TResult result;
        try {
            result = _func(transaction.ToAbstract());
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
        // RunTransactionAsync rejects a task TResult up front; a task returned through an object result means the
        // update function is asynchronous too, so abort the attempt before any of its writes commit.
        ResultException = TransactionResultGuard.GetAsyncResultException(result);
        if(ResultException != null) {
            throw ResultException;
        }
        Result = result;
        return null;
    }
}