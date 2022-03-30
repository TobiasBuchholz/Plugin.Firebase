using System;
using System.Collections.Generic;
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
            return RunAuthTask(_firebaseAuth.SignInAnonymouslyAsync(), signOutWhenFailed: true);
        }

        private IObservable<Unit> RunAuthTask(Task<IFirebaseUser> task, bool signOutWhenFailed = false)
        {
            _isSignInRunningSubject.OnNext(true);
            return Observable
                .FromAsync(_ => task)
                .Do(_currentUserSubject.OnNext)
                .ToUnit()
                .Catch<Unit, Exception>(e => (signOutWhenFailed ? SignOut() : Observables.Unit).SelectMany(Observable.Throw<Unit>(e)))
                .Finally(() => _isSignInRunningSubject.OnNext(false));
        }

        public IObservable<Unit> SignInWithEmailAndPassword(string email, string password)
        {
            return RunAuthTask(
                _firebaseAuth.SignInWithEmailAndPasswordAsync(email, password),
                signOutWhenFailed: true);
        }

        public IObservable<Unit> SignInWithEmailLink(string email, string link)
        {
            return RunAuthTask(
                _firebaseAuth.SignInWithEmailLinkAsync(email, link),
                signOutWhenFailed: true);
        }

        public IObservable<Unit> SignInWithGoogle()
        {
            return RunAuthTask(
                _firebaseAuth.SignInWithGoogleAsync(),
                signOutWhenFailed: true);
        }

        public IObservable<Unit> SignInWithFacebook()
        {
            return RunAuthTask(
                _firebaseAuth.SignInWithFacebookAsync(),
                signOutWhenFailed: true);
        }

        public IObservable<Unit> VerifyPhoneNumber(string phoneNumber)
        {
            return _firebaseAuth.VerifyPhoneNumberAsync(phoneNumber).ToObservable();
        }

        public IObservable<Unit> SignInWithPhoneNumberVerificationCode(string verificationCode)
        {
            return RunAuthTask(
                _firebaseAuth.SignInWithPhoneNumberVerificationCodeAsync(verificationCode),
                signOutWhenFailed: true);
        }

        public IObservable<Unit> LinkWithEmailAndPassword(string email, string password)
        {
            return RunAuthTask(_firebaseAuth.LinkWithEmailAndPasswordAsync(email, password));
        }

        public IObservable<Unit> LinkWithGoogle()
        {
            return RunAuthTask(_firebaseAuth.LinkWithGoogleAsync());
        }

        public IObservable<Unit> LinkWithFacebook()
        {
            return RunAuthTask(_firebaseAuth.LinkWithFacebookAsync());
        }

        public IObservable<Unit> LinkWithPhoneNumberVerificationCode(string verificationCode)
        {
            return RunAuthTask(_firebaseAuth.LinkWithPhoneNumberVerificationCodeAsync(verificationCode));
        }

        public IObservable<Unit> UnlinkProvider(string providerId)
        {
            return RunAuthTask(CurrentUser
                .UnlinkAsync(providerId)
                .ToObservable()
                .Select(_ => _firebaseAuth.CurrentUser)
                .ToTask());
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

        public IObservable<string[]> FetchSignInMethods(string email)
        {
            return _firebaseAuth.FetchSignInMethodsAsync(email).ToObservable();
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