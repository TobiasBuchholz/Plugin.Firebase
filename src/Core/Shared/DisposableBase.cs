namespace Plugin.Firebase.Core;

/// <summary>
/// Base class implementing the standard <see cref="IDisposable"/> pattern.
/// </summary>
public class DisposableBase : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~DisposableBase()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> when called from <see cref="Dispose()"/>; <c>false</c> when called from the finalizer.
    /// </param>
    public virtual void Dispose(bool disposing)
    {
        if(!_disposed) {
            if(disposing) {
                //dispose only
            }

            _disposed = true;
        }
    }
}