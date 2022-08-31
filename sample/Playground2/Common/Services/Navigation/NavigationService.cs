using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Playground.Common.Services.Scheduler;

namespace Playground.Common.Services.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly ISchedulerService _schedulerService;

    public NavigationService(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    public Task GoToAsync(string uri)
    {
        return Observable
            .FromAsync(ct => Shell.Current.GoToAsync(uri))
            .SubscribeOn(_schedulerService.Main)
            .ToTask();
    }

    public Task PopAsync()
    {
        return Observable
            .FromAsync(ct => Shell.Current.Navigation.PopAsync())
            .SubscribeOn(_schedulerService.Main)
            .ToTask();
    }

    public Task PopModalAsync()
    {
        return Observable
            .FromAsync(ct => Shell.Current.Navigation.PopModalAsync())
            .SubscribeOn(_schedulerService.Main)
            .ToTask();
    }

    public Task PopToRootAsync()
    {
        return Observable
            .FromAsync(ct => Shell.Current.Navigation.PopToRootAsync())
            .SubscribeOn(_schedulerService.Main)
            .ToTask();
    }

    public IObservable<ShellNavigatedEventArgs> NavigatedTicks =>
        Observable
            .FromEventPattern<ShellNavigatedEventArgs>(Shell.Current, nameof(Shell.Navigated))
            .Select(x => x.EventArgs);
}