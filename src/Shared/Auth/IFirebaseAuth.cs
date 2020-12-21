using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Auth
{
    public interface IFirebaseAuth : IDisposable
    {
        Task VerifyPhoneNumberAsync(string phoneNumber);
        Task<IFirebaseUser> SignInWithCustomTokenAsync(string token);
        Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode);
        Task<IFirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password);
        Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link);
        Task<IFirebaseUser> SignInWithGoogleAsync();
        Task<IFirebaseUser> SignInWithFacebookAsync();
        Task<IFirebaseUser> SignInAnonymouslyAsync();
        Task<IFirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode);
        Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password);
        Task<IFirebaseUser> LinkWithGoogleAsync();
        Task<IFirebaseUser> LinkWithFacebookAsync();

        Task<string[]> FetchSignInMethodsAsync(string email);
        Task SendSignInLink(string toEmail, ActionCodeSettings actionCodeSettings);
        Task SignOutAsync();
        
        bool IsSignInWithEmailLink(string link);
        
        IFirebaseUser CurrentUser { get; }
    }
}
