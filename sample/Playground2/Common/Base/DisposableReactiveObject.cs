using System;
using ReactiveUI;

namespace Playground.Common.Base
{
    public class DisposableReactiveObject : ReactiveObject, IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DisposableReactiveObject()
        {
            Dispose(false);
        }

        public virtual void Dispose(bool disposing)
        {
            if(!disposed) {
                if(disposing) {
                    //dispose only
                }

                disposed = true;
            }
        }
    }
}