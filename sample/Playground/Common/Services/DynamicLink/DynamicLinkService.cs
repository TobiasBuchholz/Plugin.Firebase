// using Playground.Common.Services.Auth;
using Playground.Resources;
using Plugin.Firebase.DynamicLinks;
using Plugin.Firebase.DynamicLinks.EventArgs;

namespace Playground.Common.Services.DynamicLink;

public sealed class DynamicLinkService : IDynamicLinkService
{
    private readonly IFirebaseDynamicLinks _dynamicLinks;
    // private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IPreferencesService _preferencesService;
    private readonly IUserInteractionService _userInteractionService;
    private readonly ISchedulerService _schedulerService;

    public DynamicLinkService(
        IFirebaseDynamicLinks dynamicLinks,
        // IAuthService authService,
        INavigationService navigationService,
        IPreferencesService preferencesService,
        IUserInteractionService userInteractionService,
        ISchedulerService schedulerService)
    {
        _dynamicLinks = dynamicLinks;
        // _authService = authService;
        _navigationService = navigationService;
        _preferencesService = preferencesService;
        _userInteractionService = userInteractionService;
        _schedulerService = schedulerService;
    }

    public IDisposable Register()
    {
        return Observable
            .FromEventPattern<DynamicLinkReceivedEventArgs>(_dynamicLinks, nameof(_dynamicLinks.DynamicLinkReceived))
            .Select(x => x.EventArgs.Link)
            .StartWith(_dynamicLinks.GetDynamicLink())
            .ObserveOn(_schedulerService.TaskPool)
            .WhereNotEmpty()
            .DistinctUntilChanged()
            .SelectMany(HandleDynamicLink)
            .Subscribe();
    }

    private IObservable<Unit> HandleDynamicLink(string link)
    {
        return Observables.Unit;
        // return _authService.IsSignInWithEmailLink(link)
        //     ? SignInWithEmailLink(link)
        //     : _navigationService.GoToAsync(link).ToObservable();
    }

    private IObservable<Unit> SignInWithEmailLink(string link)
    {
        return Observables.Unit;
        // return _authService
        //     .SignInWithEmailLink(_preferencesService.GetString(PreferenceKeys.SignInLinkEmail), link)
        //     .SelectMany(_ => _navigationService.GoToAsync(NavigationPaths.PageAuth).ToObservable())
        //     .DoOnError(e => _userInteractionService.ShowErrorDialogAsync(Localization.DialogTitleUnexpectedError, e))
        //     .CatchAndLogException();
    }
}