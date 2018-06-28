using System;
using System.Threading.Tasks;

namespace Plugin.Firebase.Abstractions.Auth
{
    public abstract class BaseFirebaseAuth : IFirebaseAuth
    {
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BaseFirebaseAuth()
        {
            Dispose(false);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposed) {
                if (disposing) {
                    //dispose only
                }

                disposed = true;
            }
        }

        public abstract Task VerifyPhoneNumberAsync(string phoneNumber);
        public abstract Task<FirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode);
        public abstract Task<FirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password);
        public abstract Task<FirebaseUser> SignInWithGoogleAsync();
        public abstract Task<FirebaseUser> SignInWithFacebookAsync();
        public abstract Task<FirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode);
        public abstract Task<FirebaseUser> LinkWithEmailAndPasswordAync(string email, string password);
        public abstract Task<FirebaseUser> LinkWithGoogleAsync();
        public abstract Task<FirebaseUser> LinkWithFacebookAsync();
    }
}
