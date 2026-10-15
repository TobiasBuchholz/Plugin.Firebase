using Firebase.Auth;
using Java.Lang;
using Java.Util.Concurrent;
using Plugin.Firebase.Auth.Internal;

namespace Plugin.Firebase.Auth.Platforms.Android.PhoneNumber;

public sealed class PhoneNumberAuth
{
    private readonly PhoneNumberVerificationTracker<PhoneAuthCredential, PhoneAuthProvider.ForceResendingToken>
        _verifications = new();

    public Task VerifyPhoneNumberAsync(Activity activity, string phoneNumber)
    {
        return _verifications.Start(phoneNumber, (request, resendToken) => {
            // each request gets its own callbacks, bound to that request
            var callbacks = new PhoneVerificationStateChangeCallbacks(
                onCodeSent: x => request.OnCodeSent(x.VerificationId, x.forceResendingToken),
                onVerificationCompleted: request.OnVerificationCompleted,
                onVerificationFailed: exception =>
                    request.OnVerificationFailed(FirebaseAuthExceptionFactory.Create(exception)),
                onCodeAutoRetrievalTimeOut: request.OnCodeAutoRetrievalTimeOut
            );
            var options = PhoneAuthOptions
                .NewBuilder()
                .SetPhoneNumber(phoneNumber)
                .SetTimeout(Long.ValueOf(60), TimeUnit.Seconds!)
                .SetActivity(activity)
                .SetCallbacks(callbacks);
            if(resendToken is not null) {
                // with a resend token, native skips its per-number session reuse and starts a new verification
                options.SetForceResendingToken(resendToken);
            }

            PhoneAuthProvider.VerifyPhoneNumber(options.Build());
        });
    }

    public Task<PhoneAuthCredential> GetCredentialAsync(string verificationCode)
    {
        return Task.FromResult(
            _verifications.GetCredential(verificationCode, PhoneAuthProvider.GetCredential)
        );
    }
}