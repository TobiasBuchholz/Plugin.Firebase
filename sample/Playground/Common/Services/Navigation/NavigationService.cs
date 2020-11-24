using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Playground.Common.Services.Navigation
{
    public sealed class NavigationService : INavigationService
    {
        private readonly IScheduler _mainScheduler;

        public NavigationService(IScheduler mainScheduler)
        {
            _mainScheduler = mainScheduler;
        }

        public Task GoToAsync(string uri)
        {
            return Observable
                .FromAsync(ct => Shell.Current.GoToAsync(uri))
                .SubscribeOn(_mainScheduler)
                .ToTask();
        }

        public Task PopAsync()
        {
            return Observable
                .FromAsync(ct => Shell.Current.Navigation.PopAsync())
                .SubscribeOn(_mainScheduler)
                .ToTask();
        }

        public Task PopModalAsync()
        {
            return Observable
                .FromAsync(ct => Shell.Current.Navigation.PopModalAsync())
                .SubscribeOn(_mainScheduler)
                .ToTask();
        }

        public Task PopToRootAsync()
        {
            return Observable
                .FromAsync(ct => Shell.Current.Navigation.PopToRootAsync())
                .SubscribeOn(_mainScheduler)
                .ToTask();
        }

        public IObservable<ShellNavigatedEventArgs> NavigatedTicks =>
            Observable
                .FromEventPattern<ShellNavigatedEventArgs>(Shell.Current, nameof(Shell.Navigated))
                .Select(x => x.EventArgs);
    }
}