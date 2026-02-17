namespace Plugin.Firebase.Core;

/// <summary>
/// Provides hooks for registering callbacks that execute at different stages of Firebase initialization.
/// </summary>
public static class FirebaseInitializationHooks
{
    private static readonly object SyncRoot = new();
    private static readonly List<Action> BeforeConfigureCallbacks = new();
    private static readonly List<Action> AfterInitializeCallbacks = new();

    private static bool AfterInitializeInvoked;

    /// <summary>
    /// Registers a callback to be executed before Firebase configuration.
    /// </summary>
    /// <param name="callback">The callback action to execute before configuration.</param>
    /// <returns>An <see cref="IDisposable"/> that unregisters the callback when disposed.</returns>
    public static IDisposable RegisterBeforeConfigure(Action callback)
    {
        if(callback == null) {
            throw new ArgumentNullException(nameof(callback));
        }

        lock(SyncRoot) {
            BeforeConfigureCallbacks.Add(callback);
        }

        return new CallbackRegistration(BeforeConfigureCallbacks, callback, SyncRoot);
    }

    /// <summary>
    /// Registers a callback to be executed after Firebase initialization.
    /// If initialization has already occurred, the callback is executed immediately.
    /// </summary>
    /// <param name="callback">The callback action to execute after initialization.</param>
    /// <returns>An <see cref="IDisposable"/> that unregisters the callback when disposed.</returns>
    public static IDisposable RegisterAfterInitialize(Action callback)
    {
        if(callback == null) {
            throw new ArgumentNullException(nameof(callback));
        }

        // If initialization already happened, run immediately.
        // This makes callers robust when they configure a feature (e.g., AppCheck)
        // after CrossFirebase.Initialize() has already been called.
        lock(SyncRoot) {
            if(AfterInitializeInvoked) {
                callback();
                return new NoopRegistration();
            }

            AfterInitializeCallbacks.Add(callback);
        }

        return new CallbackRegistration(AfterInitializeCallbacks, callback, SyncRoot);
    }

    internal static void InvokeBeforeConfigure()
    {
        Action[] callbacks;
        lock(SyncRoot) {
            callbacks = BeforeConfigureCallbacks.ToArray();
        }

        foreach(var callback in callbacks) {
            callback();
        }
    }

    internal static void InvokeAfterInitialize()
    {
        Action[] callbacks;
        lock(SyncRoot) {
            AfterInitializeInvoked = true;
            callbacks = AfterInitializeCallbacks.ToArray();
        }

        foreach(var callback in callbacks) {
            callback();
        }
    }

    internal static void Reset()
    {
        lock(SyncRoot) {
            BeforeConfigureCallbacks.Clear();
            AfterInitializeCallbacks.Clear();
            AfterInitializeInvoked = false;
        }
    }

    private sealed class NoopRegistration : IDisposable
    {
        public void Dispose() { }
    }

    private sealed class CallbackRegistration : IDisposable
    {
        private readonly List<Action> _target;
        private readonly Action _callback;
        private readonly object _syncRoot;
        private bool _disposed;

        public CallbackRegistration(List<Action> target, Action callback, object syncRoot)
        {
            _target = target;
            _callback = callback;
            _syncRoot = syncRoot;
        }

        public void Dispose()
        {
            if(_disposed) {
                return;
            }

            lock(_syncRoot) {
                _target.Remove(_callback);
            }

            _disposed = true;
        }
    }
}