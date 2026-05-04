namespace Plugin.Firebase.IntegrationTests;

internal sealed class CallbackProbe<T>
{
    private readonly TaskCompletionSource<T> _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsCompleted => _completion.Task.IsCompleted;

    public bool TrySetResult(T result)
    {
        return _completion.TrySetResult(result);
    }

    public bool TrySetException(Exception exception)
    {
        return _completion.TrySetException(exception);
    }

    public Task<T> WaitAsync(TimeSpan timeout, string operationName)
    {
        return _completion.Task.WaitForTestAsync(timeout, operationName);
    }
}

internal sealed class EventProbe<TEventArgs> : IDisposable
{
    private readonly CallbackProbe<TEventArgs> _probe = new();
    private readonly EventHandler<TEventArgs> _handler;
    private readonly Action<EventHandler<TEventArgs>> _unsubscribe;

    public EventProbe(
        Action<EventHandler<TEventArgs>> subscribe,
        Action<EventHandler<TEventArgs>> unsubscribe)
    {
        _handler = (_, args) => _probe.TrySetResult(args);
        _unsubscribe = unsubscribe;
        subscribe(_handler);
    }

    public Task<TEventArgs> WaitAsync(TimeSpan timeout, string operationName)
    {
        return _probe.WaitAsync(timeout, operationName);
    }

    public void Dispose()
    {
        _unsubscribe(_handler);
    }
}