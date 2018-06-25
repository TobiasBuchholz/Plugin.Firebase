using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Abstractions.Auth
{
    public interface IFirebaseAuth : IDisposable
    {
        Task<FirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password);
        Task<FirebaseUser> SignInWithGoogleAsync();
        Task<FirebaseUser> SignInWithFacebookAsync();
        Task VerifyPhoneNumberAsync(string phoneNumber);
        Task<FirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode);
    }
}
