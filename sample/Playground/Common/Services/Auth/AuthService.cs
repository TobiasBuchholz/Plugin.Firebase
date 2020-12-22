using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Playground.Common.Services.Preferences;
using Plugin.Firebase.Auth;

namespace Playground.Common.Services.Auth
{
    public sealed class AuthService : IAuthService
    {
        private readonly IFirebaseAuth _firebaseAuth;
        private readonly IPreferencesService _preferencesService;
        private readonly BehaviorSubject<IFirebaseUser> _currentUserSubject;
        private readonly ISubject<bool> _isSignInRunningSubject;
        
        public AuthService(
            IFirebaseAuth firebaseAuth,
            IPreferencesService preferencesService)
        {
            _firebaseAuth = firebaseAuth;
            _preferencesService = preferencesService;
            _currentUserSubject = new BehaviorSubject<IFirebaseUser>(null);
            _isSignInRunningSubject = new BehaviorSubject<bool>(false);
            
            _currentUserSubject.OnNext(_firebaseAuth.CurrentUser);
        }

        public IObservable<Unit> SignAnonymously()
        {
            return SignInWithTask(_firebaseAuth.SignInAnonymouslyAsync());
        }

        private IObservable<Unit> SignInWithTask(Task<IFirebaseUser> signInTask)
        {
            _isSignInRunningSubject.OnNext(true);
            return Observable
                .FromAsync(_ => signInTask)
                .Do(_currentUserSubject.OnNext)
                .ToUnit()
                .Catch<Unit, Exception>(e => SignOut().SelectMany(Observable.Throw<Unit>(e)))
                .Finally(() => _isSignInRunningSubject.OnNext(false));
        }

        public IObservable<Unit> SignInWithEmailAndPassword(string email, string password)
        {
            return SignInWithTask(_firebaseAuth.SignInWithEmailAndPasswordAsync(email, password));
        }

        public IObservable<Unit> SignInWithEmailLink(string email, string link)
        {
            return SignInWithTask(_firebaseAuth.SignInWithEmailLinkAsync(email, link));
        }

        public IObservable<Unit> SignInWithGoogle()
        {
            return SignInWithTask(_firebaseAuth.SignInWithGoogleAsync());
        }

        public IObservable<Unit> SignInWithFacebook()
        {
            return SignInWithTask(_firebaseAuth.SignInWithFacebookAsync());
        }

        public IObservable<Unit> VerifyPhoneNumber(string phoneNumber)
        {
            return _firebaseAuth.VerifyPhoneNumberAsync(phoneNumber).ToObservable();
        }

        public IObservable<Unit> SignInWithPhoneNumberVerificationCode(string verificationCode)
        {
            return SignInWithTask(_firebaseAuth.SignInWithPhoneNumberVerificationCodeAsync(verificationCode));
        }

        public IObservable<Unit> SendSignInLink(string toEmail)
        {
            return _firebaseAuth
                .SendSignInLink(toEmail, CreateActionCodeSettings())
                .ToObservable()
                .Do(_ => _preferencesService.Set(PreferenceKeys.SignInLinkEmail, toEmail));
        }

        private static ActionCodeSettings CreateActionCodeSettings()
        {
            var settings = new ActionCodeSettings();
            settings.Url = "https://playground-24cec.firebaseapp.com";
            settings.HandleCodeInApp = true;
            settings.IOSBundleId = "com.tobishiba.playground";
            settings.SetAndroidPackageName("com.tobishiba.playground", true, "21");
            return settings;
        }
        
        public IObservable<Unit> SignOut()
        {
            return _firebaseAuth
                .SignOutAsync()
                .ToObservable()
                .Do(_ => HandleUserSignedOut());
        }

        private void HandleUserSignedOut()
        {
            _currentUserSubject.OnNext(null);
            _preferencesService.Remove(PreferenceKeys.SignInLinkEmail);
        }

        public bool IsSignInWithEmailLink(string link)
        {
            return _firebaseAuth.IsSignInWithEmailLink(link);
        }
        
        public IFirebaseUser CurrentUser => _currentUserSubject.Value;
        public IObservable<IFirebaseUser> CurrentUserTicks => _currentUserSubject.AsObservable();
        public IObservable<bool> IsSignedInTicks => CurrentUserTicks.Select(x => x != null);
        public IObservable<bool> IsSignInRunningTicks => _isSignInRunningSubject.AsObservable();
    }
}