using Firebase.Auth;
using Java.Lang;
using Java.Util.Concurrent;

namespace Plugin.Firebase.Auth.Platforms.Android.PhoneNumber;

public sealed class PhoneNumberAuth
{
    private string? _verificationId;

    public Task VerifyPhoneNumberAsync(Activity activity, string phoneNumber)
    {
        var callbacks = new PhoneVerificationStateChangeCallbacks(onCodeSent: x =>
            _verificationId = x.VerificationId
        );
        var options = PhoneAuthOptions
            .NewBuilder()
            .SetPhoneNumber(phoneNumber)
            .SetTimeout(Long.ValueOf(60), TimeUnit.Seconds!)
            .SetActivity(activity)
            .SetCallbacks(callbacks)
            .Build();

        PhoneAuthProvider.VerifyPhoneNumber(options);
        return Task.CompletedTask;
    }

    public Task<PhoneAuthCredential> GetCredentialAsync(string verificationCode)
    {
        if(_verificationId is null) {
            throw new InvalidOperationException(
                "VerifyPhoneNumberAsync must be called before GetCredentialAsync."
            );
        }
        return Task.FromResult(PhoneAuthProvider.GetCredential(_verificationId, verificationCode));
    }
}