namespace Playground.Common.Services.Navigation;

public interface INavigationService
{
    Task GoToAsync(string uri);
    Task PopAsync();
    Task PopModalAsync();
    Task PopToRootAsync();

    IObservable<ShellNavigatedEventArgs> NavigatedTicks { get; }
}