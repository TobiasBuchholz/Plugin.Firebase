using System.Collections.Generic;
using Playground.Common.Base;
using ReactiveUI;

namespace Playground.Common.Services.Composition
{
    public sealed class ViewModelResolver
    {
        public static void Initialize(CompositionRootBase compositionRootBase)
        {
            if(Instance == null) {
                Instance = new ViewModelResolver(compositionRootBase);
            }
        }
        
        public static ViewModelResolver Instance { get; private set; }

        private readonly CompositionRootBase _compositionRoot;
        
        private ViewModelResolver(CompositionRootBase compositionRoot)
        {
            _compositionRoot = compositionRoot;
        }


        public ViewModelBase Resolve(IViewFor viewFor, IEnumerable<object> parameters = null)
        {
            return _compositionRoot.ResolveViewModel(viewFor, parameters);
        }
    }
}