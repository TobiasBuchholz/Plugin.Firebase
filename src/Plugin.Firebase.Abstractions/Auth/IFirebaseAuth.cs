using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Abstractions.Auth
{
    public interface IFirebaseAuth : IDisposable
    {
        Task VerifyPhoneNumberAsync(string phoneNumber);
        Task<FirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode);
        Task<FirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password);
        Task<FirebaseUser> SignInWithGoogleAsync();
        Task<FirebaseUser> SignInWithFacebookAsync();
        Task<FirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode);
        Task<FirebaseUser> LinkWithEmailAndPasswordAync(string email, string password);
        Task<FirebaseUser> LinkWithGoogleAsync();
        Task<FirebaseUser> LinkWithFacebookAsync();
    }
}
