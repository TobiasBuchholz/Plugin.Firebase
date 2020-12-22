using System;
using System.Reactive;
using Plugin.Firebase.Auth;

namespace Playground.Common.Services.Auth
{
    public interface IAuthService
    {
        IObservable<Unit> SignAnonymously();
        IObservable<Unit> SignInWithEmailAndPassword(string email, string password);
        IObservable<Unit> SendSignInLink(string toEmail);
        IObservable<Unit> SignInWithEmailLink(string email, string link);
        IObservable<Unit> SignInWithGoogle();
        IObservable<Unit> SignInWithFacebook();
        IObservable<Unit> VerifyPhoneNumber(string phoneNumber);
        IObservable<Unit> SignInWithPhoneNumberVerificationCode(string verificationCode);
        IObservable<Unit> SignOut();

        bool IsSignInWithEmailLink(string link);
        IFirebaseUser CurrentUser { get; }
        
        IObservable<IFirebaseUser> CurrentUserTicks { get; }
        IObservable<bool> IsSignedInTicks { get; }
        IObservable<bool> IsSignInRunningTicks { get; }
    }
}