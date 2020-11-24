using System;
using System.Collections.Generic;
using Playground.Common.Services.Composition;
using ReactiveUI.XamForms;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Playground.Common.Base
{
    public abstract class ContentPageBase<TViewModel> : ReactiveContentPage<TViewModel> 
        where TViewModel : class
    {
        protected static Func<bool, bool> Negate => x => !x;
        
        private readonly IList<object> _queryParameters;
        
        protected ContentPageBase()
        {
            _queryParameters = new List<object>();
            
            Visual = VisualMarker.Material;
            On<iOS>().SetUseSafeArea(true);
        }

        /// <summary>
        /// Use this method to collect all parameters that are passed via [QueryProperty] annotation.
        /// </summary>
        protected void CollectQueryParameter(object parameter)
        {
            _queryParameters.Add(parameter);
        }

        protected void CollectQueryParameter(string parameter)
        {
            var unescapeDataString = Uri.UnescapeDataString(parameter);
            var urlConformString = unescapeDataString.FixBrokenUrl();
            _queryParameters.Add(urlConformString);
        }

        /// <summary>
        /// Call this method right after InitializeComponent() to instantiate the ViewModel of the ContentPage OR after
        /// all query parameters have been collected via CollectQueryParameter() method. The order of the [QueryProperty]
        /// annotations dictate the order of collection, so call InitializeViewModel() in the setter of the last
        /// query property.
        /// </summary>
        protected void InitializeViewModel()
        {
            BindingContext = ViewModelResolver.Instance.Resolve(this, _queryParameters);
        }
    }
}