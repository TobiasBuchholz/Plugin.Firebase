using System;

namespace Plugin.Firebase.Common
{
    public class DisposableBase : IDisposable
    {
        private bool disposed = false;

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
            if (!disposed) {
                if (disposing) {
                    //dispose only
                }

                disposed = true;
            }
        }  
    }
}