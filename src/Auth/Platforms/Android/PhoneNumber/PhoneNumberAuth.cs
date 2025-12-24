using Firebase.Auth;
using Java.Lang;
using Java.Util.Concurrent;

namespace Plugin.Firebase.Auth.Platforms.Android.PhoneNumber;

public sealed class PhoneNumberAuth
{
    private string _verificationId;

    public Task VerifyPhoneNumberAsync(Activity activity, string phoneNumber)
    {
        var callbacks = new PhoneVerificationStateChangeCallbacks(onCodeSent: x => _verificationId = x.VerificationId);
        var options = PhoneAuthOptions
            .NewBuilder()
            .SetPhoneNumber(phoneNumber)
            .SetTimeout(new Long(60), TimeUnit.Seconds)
            .SetActivity(activity)
            .SetCallbacks(callbacks)
            .Build();

        PhoneAuthProvider.VerifyPhoneNumber(options);
        return Task.CompletedTask;
    }

    public Task<PhoneAuthCredential> GetCredentialAsync(string verificationCode)
    {
        return Task.FromResult(PhoneAuthProvider.GetCredential(_verificationId, verificationCode));
    }
}