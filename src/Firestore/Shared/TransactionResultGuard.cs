namespace Plugin.Firebase.Firestore;

/// <summary>
/// Rejects asynchronous update functions passed to <see cref="IFirebaseFirestore.RunTransactionAsync{TResult}"/>.
/// The native SDKs run the update function synchronously and commit the transaction as soon as it returns, so an
/// async update function would commit only the writes made before its first <c>await</c>, and the rest of it would
/// run outside the transaction.
/// </summary>
internal static class TransactionResultGuard
{
    private const string UpdateFuncParameterName = "updateFunc";

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> when <paramref name="resultType"/>, the <c>TResult</c> of
    /// <c>RunTransactionAsync</c>, is a task type, which is what the compiler infers for an <c>async</c> update function.
    /// Called before the native transaction starts, so the update function never runs.
    /// </summary>
    public static void ThrowIfAsyncResultType(Type resultType)
    {
        if(IsAsyncResultType(resultType)) {
            throw CreateAsyncResultException();
        }
    }

    /// <summary>
    /// Returns the <see cref="ArgumentException"/> to abort the attempt with when the value returned by the update
    /// function is a task, for example one returned through an <see cref="object"/> result; otherwise
    /// <see langword="null"/>.
    /// </summary>
    public static ArgumentException? GetAsyncResultException(object? result)
    {
        return result != null && IsAsyncResultType(result.GetType()) ? CreateAsyncResultException() : null;
    }

    internal static bool IsAsyncResultType(Type type)
    {
        return typeof(Task).IsAssignableFrom(type)
            || type == typeof(ValueTask)
            || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>);
    }

    private static ArgumentException CreateAsyncResultException()
    {
        return new ArgumentException(
            "The update function passed to RunTransactionAsync must be synchronous, but it returned a Task or ValueTask. "
                + "The native SDK commits the transaction as soon as the update function returns, so an async update "
                + "function would commit only the writes made before its first await. Do asynchronous work before or "
                + "after the transaction.",
            UpdateFuncParameterName);
    }
}