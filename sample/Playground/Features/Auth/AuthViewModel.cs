using System;
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

namespace Playground.Features.Auth
{
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
            SignOutCommand = ReactiveCommand.CreateFromObservable(SignOut, canSignOut);
           
            Observable
                .Merge(
                    SignInAnonymouslyCommand.ThrownExceptions,
                    SignInWithEmailCommand.ThrownExceptions,
                    SignInWithEmailLinkCommand.ThrownExceptions,
                    SignInWithGoogleCommand.ThrownExceptions,
                    SignInWithFacebookCommand.ThrownExceptions,
                    SignInWithPhoneNumberCommand.ThrownExceptions,
                    SignOutCommand.ThrownExceptions)
                .LogThrownException()
                .Subscribe(e => _userInteractionService.ShowErrorDialogAsync(Localization.DialogTitleUnexpectedError, e))
                .DisposeWith(Disposables);
        }

        private IObservable<Unit> SignInAnonymously()
        {
            return _authService.SignAnonymously();
        }

        private async Task<Unit> SignInWithEmailAsync()
        {
            var email = await AskForEmailAsync();
            var password = await AskForPasswordAsync();
            return await _authService.SignInWithEmailAndPassword(email, password).ToTask();
        }

        private Task<string> AskForEmailAsync()
        {
            return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
                .WithMessage(Localization.DialogMessageEnterEmail)
                .WithDefaultButton(Localization.Continue)
                .WithCancelButton(Localization.Cancel)
                .Build());
        }
        
        private Task<string> AskForPasswordAsync()
        {
            return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
                .WithMessage(Localization.DialogMessageEnterPassword)
                .WithDefaultButton(Localization.ButtonTextSignIn)
                .WithCancelButton(Localization.Cancel)
                .Build());
        }

        private async Task SignInWithEmailLinkAsync()
        {
            var email = await AskForEmailAsync();
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
                .WithDefaultButton(Localization.ButtonTextSendVerificationCode)
                .WithCancelButton(Localization.Cancel)
                .Build());
        }

        private IObservable<Unit> SignInWithPhoneNumber(string phoneNumber)
        {
            return _authService
                .VerifyPhoneNumber(phoneNumber)
                .SelectMany(_ => AskForVerificationCodeAsync())
                .SelectMany(x => string.IsNullOrEmpty(x) ? null : _authService.SignInWithPhoneNumberVerificationCode(x));
        }

        private Task<string> AskForVerificationCodeAsync()
        {
            return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
                .WithMessage(Localization.DialogMessageEnterVerificationCode)
                .WithDefaultButton(Localization.ButtonTextSignIn)
                .WithCancelButton(Localization.Cancel)
                .Build());
        }

        private IObservable<Unit> SignOut()
        {
            return _authService.SignOut();
        }

        private void InitProperties()
        {
            InitUserProperty();
            InitLoginTextProperty();
        }

        private void InitUserProperty()
        {
            _authService
                .CurrentUserTicks
                .ToPropertyEx(this, x => x.User)
                .DisposeWith(Disposables);
        }

        private void InitLoginTextProperty()
        {
            this.WhenAnyValue(x => x.User)
                .Select(x => x == null ? Localization.LabelUserIsSignedOut : Localization.LabelUserIsSignedIn.WithParams(x.Email))
                .ToPropertyEx(this, x => x.LoginText)
                .DisposeWith(Disposables);
        }

        private extern IFirebaseUser User { [ObservableAsProperty] get; }
        public extern string LoginText { [ObservableAsProperty] get; }
        
        public ReactiveCommand<Unit, Unit> SignInAnonymouslyCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignInWithEmailCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignInWithEmailLinkCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignInWithGoogleCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignInWithFacebookCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignInWithPhoneNumberCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignOutCommand { get; set; }
    }
}