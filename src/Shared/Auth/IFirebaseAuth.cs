using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Auth
{
    public interface IFirebaseAuth : IDisposable
    {
        Task VerifyPhoneNumberAsync(string phoneNumber);
        Task<FirebaseUser> SignInWithCustomTokenAsync(string token);
        Task<FirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode);
        Task<FirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password);
        Task<FirebaseUser> SignInWithEmailLinkAsync(string email, string link);
        Task<FirebaseUser> SignInWithGoogleAsync();
        Task<FirebaseUser> SignInWithFacebookAsync();
        Task<FirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode);
        Task<FirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password);
        Task<FirebaseUser> LinkWithGoogleAsync();
        Task<FirebaseUser> LinkWithFacebookAsync();

        Task<string[]> FetchSignInMethodsAsync(string email);
        Task SendSignInLink(string toEmail, ActionCodeSettings actionCodeSettings);
        Task SignOutAsync();
        
        bool IsSignInWithEmailLink(string link);
        
        FirebaseUser CurrentUser { get; }
    }
}
