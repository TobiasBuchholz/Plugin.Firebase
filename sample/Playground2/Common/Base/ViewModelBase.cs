using System.Reactive.Disposables;

namespace Playground.Common.Base
{
    public class ViewModelBase : DisposableReactiveObject
    {
        protected ViewModelBase()
        {
            Disposables = new CompositeDisposable();
        }

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Disposables.Dispose();
        }

        protected CompositeDisposable Disposables { get; }
    }
}