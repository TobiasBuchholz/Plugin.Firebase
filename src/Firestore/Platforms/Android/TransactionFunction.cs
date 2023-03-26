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

    public Object Apply(Transaction transaction)
    {
        return _func(transaction.ToAbstract()).ToJavaObject();
    }
}