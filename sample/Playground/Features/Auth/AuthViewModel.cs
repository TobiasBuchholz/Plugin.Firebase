using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Playground.Common.Base;
using Playground.Common.Extensions;
using Playground.Common.Services.Auth;
using Playground.Common.Services.Scheduler;
using Playground.Common.Services.UserInteraction;
using Playground.Resources;
using Plugin.Firebase.Auth;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Xamarin.Forms.Internals;

namespace Playground.Features.Auth
{
    [Preserve(AllMembers = true)]
    public sealed class AuthViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IUserInteractionService _userInteractionService;
        private readonly ISchedulerService _schedulerService;

        public AuthViewModel(
            IAuthService authService,
            IUserInteractionService userInteractionService,
            ISchedulerService schedulerService)
        {
            _authService = authService;
            _userInteractionService = userInteractionService;
            _schedulerService = schedulerService;

            InitCommands();
            InitProperties();
        }

        private void InitCommands()
        {
            var canSignIn = this.WhenAnyValue(x => x.User).Select(x => x == null).ObserveOn(_schedulerService.Main);
            var canSignOut = this.WhenAnyValue(x => x.User).Select(x => x != null).ObserveOn(_schedulerService.Main);

            SignInAnonymouslyCommand = ReactiveCommand.CreateFromObservable(SignInAnonymously, canSignIn);
            SignInWithEmailCommand = ReactiveCommand.CreateFromTask(SignInWithEmailAsync, canSignIn);
            SignInWithEmailLinkCommand = ReactiveCommand.CreateFromTask(SignInWithEmailLinkAsync, canSignIn);
            SignInWithGoogleCommand = ReactiveCommand.CreateFromObservable(SignInWithGoogle, canSignIn);
            SignInWithFacebookCommand = ReactiveCommand.CreateFromObservable(SignInWithFacebook, canSignIn);
            SignInWithPhoneNumberCommand = ReactiveCommand.CreateFromObservable(SignInWithPhoneNumber, canSignIn);
            LinkWithEmailCommand = ReactiveCommand.CreateFromTask(LinkWithEmailAsync);
            LinkWithGoogleCommand = ReactiveCommand.CreateFromObservable(LinkWithGoogle);
            LinkWithFacebookCommand = ReactiveCommand.CreateFromObservable(LinkWithFacebook);
            LinkWithPhoneNumberCommand = ReactiveCommand.CreateFromObservable(LinkWithPhoneNumber);
            UnlinkProviderCommand = ReactiveCommand.CreateFromObservable(UnlinkProvider);
            SignOutCommand = ReactiveCommand.CreateFromObservable(SignOut, canSignOut);

            Observable
                .Merge(
                    SignInAnonymouslyCommand.ThrownExceptions,
                    SignInWithEmailCommand.ThrownExceptions,
                    SignInWithEmailLinkCommand.ThrownExceptions,
                    SignInWithGoogleCommand.ThrownExceptions,
                    SignInWithFacebookCommand.ThrownExceptions,
                    SignInWithPhoneNumberCommand.ThrownExceptions,
                    LinkWithEmailCommand.ThrownExceptions,
                    LinkWithGoogleCommand.ThrownExceptions,
                    LinkWithFacebookCommand.ThrownExceptions,
                    LinkWithPhoneNumberCommand.ThrownExceptions,
                    UnlinkProviderCommand.ThrownExceptions,
                    SignOutCommand.ThrownExceptions)
                .LogThrownException()
                .Subscribe(e => _userInteractionService.ShowErrorDialogAsync(Localization.DialogTitleUnexpectedError, e))
                .DisposeWith(Disposables);
        }

        private IObservable<Unit> SignInAnonymously()
        {
            return _authService.SignAnonymously();
        }

        private async Task SignInWithEmailAsync()
        {
            var email = await AskForEmailAsync();
            if(!string.IsNullOrEmpty(email)) {
                await SignInWithEmailAsync(email);
            }
        }

        private Task<string> AskForEmailAsync()
        {
            return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
                .WithMessage(Localization.DialogMessageEnterEmail)
                .WithDefaultButton(Localization.Continue)
                .WithCancelButton(Localization.Cancel)
                .Build());
        }

        private async Task SignInWithEmailAsync(string email)
        {
            var password = await AskForPasswordAsync();
            if(!string.IsNullOrEmpty(password)) {
                await _authService.SignInWithEmailAndPassword(email, password).ToTask();
            }
        }

        private Task<string> AskForPasswordAsync()
        {
            return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
                .WithMessage(Localization.DialogMessageEnterPassword)
                .WithDefaultButton(Localization.ButtonSignIn)
                .WithCancelButton(Localization.Cancel)
                .Build());
        }

        private async Task SignInWithEmailLinkAsync()
        {
            var email = await AskForEmailAsync();
            if(!string.IsNullOrEmpty(email)) {
                await SignInWithEmailLinkAsync(email);
            }
        }

        private async Task SignInWithEmailLinkAsync(string email)
        {
            await _authService.SendSignInLink(email);
            await _userInteractionService.ShowDefaultDialogAsync(Localization.DialogTitleSignInLinkSent, Localization.DialogMessageSignInLinkSent);
        }

        private IObservable<Unit> SignInWithGoogle()
        {
            return _authService.SignInWithGoogle();
        }

        private IObservable<Unit> SignInWithFacebook()
        {
            return _authService.SignInWithFacebook();
        }

        private IObservable<Unit> SignInWithPhoneNumber()
        {
            return AskForPhoneNumberAsync()
                .ToObservable()
                .SelectMany(x => string.IsNullOrEmpty(x) ? Observables.Unit : SignInWithPhoneNumber(x));
        }

        private Task<string> AskForPhoneNumberAsync()
        {
            return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
                .WithMessage(Localization.DialogMessageEnterPhoneNumber)
                .WithDefaultButton(Localization.ButtonSendVerificationCode)
                .WithCancelButton(Localization.Cancel)
                .Build());
        }

        private IObservable<Unit> SignInWithPhoneNumber(string phoneNumber)
        {
            return Observable
                .Defer(() => _authService.VerifyPhoneNumber(phoneNumber))
                .SubscribeOn(_schedulerService.Main)
                .SelectMany(_ => AskForVerificationCodeAsync())
                .SelectMany(x => string.IsNullOrEmpty(x) ? Observables.Unit : _authService.SignInWithPhoneNumberVerificationCode(x));
        }

        private Task<string> AskForVerificationCodeAsync()
        {
            return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
                .WithMessage(Localization.DialogMessageEnterVerificationCode)
                .WithDefaultButton(Localization.ButtonSignIn)
                .WithCancelButton(Localization.Cancel)
                .Build());
        }

        private async Task LinkWithEmailAsync()
        {
            var email = await AskForEmailAsync();
            if(!string.IsNullOrEmpty(email)) {
                await LinkWithEmailAsync(email);
            }
        }

        private async Task LinkWithEmailAsync(string email)
        {
            var password = await AskForPasswordAsync();
            if(!string.IsNullOrEmpty(password)) {
                await _authService.LinkWithEmailAndPassword(email, password).ToTask();
            }
        }

        private IObservable<Unit> LinkWithGoogle()
        {
            return _authService.LinkWithGoogle();
        }

        private IObservable<Unit> LinkWithFacebook()
        {
            return _authService.LinkWithFacebook();
        }

        private IObservable<Unit> LinkWithPhoneNumber()
        {
            return AskForPhoneNumberAsync()
                .ToObservable()
                .SelectMany(x => string.IsNullOrEmpty(x) ? Observables.Unit : LinkWithPhoneNumber(x));
        }

        private IObservable<Unit> LinkWithPhoneNumber(string phoneNumber)
        {
            return Observable
                .Defer(() => _authService.VerifyPhoneNumber(phoneNumber))
                .SubscribeOn(_schedulerService.Main)
                .SelectMany(_ => AskForVerificationCodeAsync())
                .SelectMany(x => string.IsNullOrEmpty(x) ? null : _authService.LinkWithPhoneNumberVerificationCode(x));
        }

        private IObservable<Unit> UnlinkProvider()
        {
            return AskForProviderIdAsync()
                .ToObservable()
                .SelectMany(x => _authService.UnlinkProvider(x))
                .Catch<Unit, ArgumentException>(_ => Observables.Unit);
        }

        private async Task<string> AskForProviderIdAsync()
        {
            var builder = new UserInfoBuilder().WithTitle(Localization.DialogTitleUnlinkProvider).WithMessage(Localization.DialogMessageUnlinkProvider);
            var providerIds = _authService.CurrentUser.ProviderInfos.Select(x => x.ProviderId).ToList();
            providerIds.ForEach(x => builder.WithDefaultButton(x));

            var index = await _userInteractionService.ShowAsActionSheetAsync(builder.Build());
            return index >= 0 ? providerIds[index] : throw new ArgumentException("No provider selected");
        }

        private IObservable<Unit> SignOut()
        {
            return _authService.SignOut();
        }

        private void InitProperties()
        {
            InitUserProperty();
            InitShowsSignInButtonsProperty();
            InitShowsLinkingButtons();
            InitShowsSignOutButtonProperty();
            InitIsInProgressProperty();
            InitLoginTextProperty();
        }

        private void InitUserProperty()
        {
            _authService
                .CurrentUserTicks
                .ToPropertyEx(this, x => x.User)
                .DisposeWith(Disposables);
        }

        private void InitShowsSignInButtonsProperty()
        {
            this.WhenAnyValue(x => x.User, x => x.IsInProgress)
                .Select(x => x.Item1 == null && !x.Item2)
                .ToPropertyEx(this, x => x.ShowsSignInButtons)
                .DisposeWith(Disposables);
        }

        private void InitShowsLinkingButtons()
        {
            this.WhenAnyValue(x => x.User, x => x.IsInProgress)
                .Select(x => (x.Item1?.IsAnonymous ?? false) && !x.Item2)
                .ToPropertyEx(this, x => x.ShowsLinkingButtons)
                .DisposeWith(Disposables);
        }

        private void InitShowsSignOutButtonProperty()
        {
            this.WhenAnyValue(x => x.User, x => x.IsInProgress)
                .Select(x => x.Item1 != null && !x.Item2)
                .ToPropertyEx(this, x => x.ShowsSignOutButtons)
                .DisposeWith(Disposables);
        }

        private void InitIsInProgressProperty()
        {
            Observable
                .Merge(
                    _authService.IsSignInRunningTicks,
                    this.WhenAnyObservable(x => x.SignInWithEmailLinkCommand.IsExecuting),
                    this.WhenAnyObservable(x => x.SignInWithPhoneNumberCommand.IsExecuting),
                    this.WhenAnyObservable(x => x.LinkWithPhoneNumberCommand.IsExecuting))
                .ToPropertyEx(this, x => x.IsInProgress)
                .DisposeWith(Disposables);
        }

        private void InitLoginTextProperty()
        {
            this.WhenAnyValue(x => x.User)
                .SelectMany(GetLoginText)
                .ToPropertyEx(this, x => x.LoginText)
                .DisposeWith(Disposables);
        }

        private IObservable<string> GetLoginText(IFirebaseUser user)
        {
            if(user == null) {
                return Observable.Return(Localization.LabelUserIsSignedOut);
            } else if(string.IsNullOrEmpty(user.Email)) {
                return Observable.Return(Localization.LabelUserIsSignedInWithoutEmail);
            } else {
                return _authService
                    .FetchSignInMethods(user.Email)
                    .Select(x => Localization.LabelUserIsSignedIn.WithParams(user.Email, x?.FirstOrDefault()));
            }
        }

        private extern IFirebaseUser User { [ObservableAsProperty] get; }
        public extern string LoginText { [ObservableAsProperty] get; }
        public extern bool ShowsSignInButtons { [ObservableAsProperty] get; }
        public extern bool ShowsLinkingButtons { [ObservableAsProperty] get; }
        public extern bool ShowsSignOutButtons { [ObservableAsProperty] get; }
        public extern bool IsInProgress { [ObservableAsProperty] get; }

        public ReactiveCommand<Unit, Unit> SignInAnonymouslyCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignInWithEmailCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignInWithEmailLinkCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignInWithGoogleCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignInWithFacebookCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignInWithPhoneNumberCommand { get; set; }
        public ReactiveCommand<Unit, Unit> LinkWithEmailCommand { get; set; }
        public ReactiveCommand<Unit, Unit> LinkWithGoogleCommand { get; set; }
        public ReactiveCommand<Unit, Unit> LinkWithFacebookCommand { get; set; }
        public ReactiveCommand<Unit, Unit> LinkWithPhoneNumberCommand { get; set; }
        public ReactiveCommand<Unit, Unit> UnlinkProviderCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignOutCommand { get; set; }
    }
}