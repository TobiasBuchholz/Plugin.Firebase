using System;
using Firebase.Firestore;
using Plugin.Firebase.Android.Extensions;
using Plugin.Firebase.Firestore;
using Object = Java.Lang.Object;

namespace Plugin.Firebase.Android.Firestore
{
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
}