using Firebase.Firestore;
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
    /// invocation, or <see langword="null"/> when the function completed. Reset on each attempt so a
    /// failed attempt followed by a successful retry does not report a stale failure.
    /// </summary>
    internal Exception? UpdateException { get; private set; }

    public Object? Apply(Transaction transaction)
    {
        UpdateException = null;
        try {
            return _func(transaction.ToAbstract()).ToJavaObject();
        } catch(Exception e) {
            UpdateException = e;
            throw;
        }
    }
}