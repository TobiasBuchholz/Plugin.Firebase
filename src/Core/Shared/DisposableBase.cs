namespace Plugin.Firebase.Core;

public class DisposableBase : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~DisposableBase()
    {
        Dispose(false);
    }

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