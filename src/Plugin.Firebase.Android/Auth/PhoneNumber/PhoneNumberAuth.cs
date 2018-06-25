using System.Threading.Tasks;
using Android.App;
using Firebase.Auth;
using Java.Util.Concurrent;

namespace Plugin.Firebase.Android.Auth.PhoneNumber
{
    public sealed class PhoneNumberAuth
    {
        private string _verificationId;
        
        public Task VerifyPhoneNumberAsync(Activity activity, string phoneNumber)
        {
            var callbacks = new PhoneVerificationStateChangeCallbacks(onCodeSent: x => _verificationId = x.VerificationId);
            PhoneAuthProvider.Instance.VerifyPhoneNumber(phoneNumber, 60, TimeUnit.Seconds, activity, callbacks);
            return Task.CompletedTask;
        }

        public Task<PhoneAuthCredential> GetCredentialAsync(string verificationCode)
        {
            return Task.FromResult(PhoneAuthProvider.GetCredential(_verificationId, verificationCode));
        }
    }
}