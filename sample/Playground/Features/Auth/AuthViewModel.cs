using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Playground.Common.Base;
using Playground.Common.Extensions;
using Playground.Common.Services.UserInteraction;
using Playground.Resources;
using Plugin.Firebase.Auth;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Playground.Features.Auth
{
    public sealed class AuthViewModel : ViewModelBase
    {
        private readonly IUserInteractionService _userInteractionService;
        private readonly IFirebaseAuth _firebaseAuth;
        
        public AuthViewModel(
            IUserInteractionService userInteractionService,
            IFirebaseAuth firebaseAuth)
        {
            _userInteractionService = userInteractionService;
            _firebaseAuth = firebaseAuth;

            InitCommands();
            InitProperties();
        }

        private void InitCommands()
        {
            var canSignIn = this.WhenAnyValue(x => x.User).Select(x => x == null);
            var canSignOut = this.WhenAnyValue(x => x.User).Select(x => x != null);
            
            SignInAnonymouslyCommand = ReactiveCommand.CreateFromTask(SignInAnonymouslyAsync, canSignIn);
            SignInWithEmailCommand = ReactiveCommand.CreateFromTask(SignInWithEmailAsync, canSignIn);
            SignInWithGoogleCommand = ReactiveCommand.CreateFromTask(SignInWithGoogleAsync, canSignIn);
            SignInWithFacebookCommand = ReactiveCommand.CreateFromTask(SignInWithFacebookAsync, canSignIn);
            SignInWithPhoneNumberCommand = ReactiveCommand.CreateFromTask(SignInWithPhoneNumberAsync, canSignIn);
            SignOutCommand = ReactiveCommand.CreateFromTask(() => _firebaseAuth.SignOutAsync(), canSignOut);
           
            Observable
                .Merge(
                    SignInAnonymouslyCommand.ThrownExceptions,
                    SignInWithEmailCommand.ThrownExceptions,
                    SignInWithGoogleCommand.ThrownExceptions,
                    SignInWithFacebookCommand.ThrownExceptions,
                    SignInWithPhoneNumberCommand.ThrownExceptions,
                    SignOutCommand.ThrownExceptions)
                .LogThrownException()
                .Subscribe(e => _userInteractionService.ShowErrorDialogAsync(Localization.DialogTitleUnexpectedError, e))
                .DisposeWith(Disposables);
        }

        private Task<IFirebaseUser> SignInAnonymouslyAsync()
        {
            return _firebaseAuth.SignInAnonymouslyAsync();
        }

        private async Task<IFirebaseUser> SignInWithEmailAsync()
        {
            var email = await AskForEmailAsync();
            var password = await AskForPasswordAsync();
            return await _firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
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

        private Task<IFirebaseUser> SignInWithGoogleAsync()
        {
            return _firebaseAuth.SignInWithGoogleAsync();
        }

        private Task<IFirebaseUser> SignInWithFacebookAsync()
        {
            return _firebaseAuth.SignInWithFacebookAsync();
        }

        private async Task<IFirebaseUser> SignInWithPhoneNumberAsync()
        {
            var phoneNumber = await AskForPhoneNumberAsync();
            return string.IsNullOrEmpty(phoneNumber) ? null : await SignInWithPhoneNumberAsync(phoneNumber);
        }

        private Task<string> AskForPhoneNumberAsync()
        {
            return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
                .WithMessage(Localization.DialogMessageEnterPhoneNumber)
                .WithDefaultButton(Localization.ButtonTextSendVerificationCode)
                .WithCancelButton(Localization.Cancel)
                .Build());
        }

        private async Task<IFirebaseUser> SignInWithPhoneNumberAsync(string phoneNumber)
        {
            await _firebaseAuth.VerifyPhoneNumberAsync(phoneNumber);
            var verificationCode = await AskForVerificationCodeAsync();
            return string.IsNullOrEmpty(verificationCode) ? null : await _firebaseAuth.SignInWithPhoneNumberVerificationCodeAsync(verificationCode);
        }

        private Task<string> AskForVerificationCodeAsync()
        {
            return _userInteractionService.ShowAsPromptAsync(new UserInfoBuilder()
                .WithMessage(Localization.DialogMessageEnterVerificationCode)
                .WithDefaultButton(Localization.ButtonTextSignIn)
                .WithCancelButton(Localization.Cancel)
                .Build());
        }

        private void InitProperties()
        {
            InitUserProperty();
            InitLoginTextProperty();
        }

        private void InitUserProperty()
        {
            Observable
                .Merge(
                    this.WhenAnyObservable(x => x.SignInAnonymouslyCommand),
                    this.WhenAnyObservable(x => x.SignInWithEmailCommand),
                    this.WhenAnyObservable(x => x.SignInWithGoogleCommand),
                    this.WhenAnyObservable(x => x.SignInWithFacebookCommand),
                    this.WhenAnyObservable(x => x.SignInWithPhoneNumberCommand),
                    this.WhenAnyObservable(x => x.SignOutCommand).Select(_ => _firebaseAuth.CurrentUser))
                .StartWith(_firebaseAuth.CurrentUser)
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
        
        public ReactiveCommand<Unit, IFirebaseUser> SignInAnonymouslyCommand { get; set; }
        public ReactiveCommand<Unit, IFirebaseUser> SignInWithEmailCommand { get; set; }
        public ReactiveCommand<Unit, IFirebaseUser> SignInWithGoogleCommand { get; set; }
        public ReactiveCommand<Unit, IFirebaseUser> SignInWithFacebookCommand { get; set; }
        public ReactiveCommand<Unit, IFirebaseUser> SignInWithPhoneNumberCommand { get; set; }
        public ReactiveCommand<Unit, Unit> SignOutCommand { get; set; }
    }
}