using Firebase.Auth;
using Java.Lang;
using Java.Util.Concurrent;
using Plugin.Firebase.Auth.Internal;

namespace Plugin.Firebase.Auth.Platforms.Android.PhoneNumber;

public sealed class PhoneNumberAuth
{
    private readonly PhoneNumberVerificationTracker<PhoneAuthCredential> _verifications = new();

    public Task VerifyPhoneNumberAsync(Activity activity, string phoneNumber)
    {
        return _verifications.Start(phoneNumber, session => {
            // each request gets its own callbacks bound to the native session of its phone number
            var callbacks = new PhoneVerificationStateChangeCallbacks(
                onCodeSent: x => session.OnCodeSent(x.VerificationId),
                onVerificationCompleted: session.OnVerificationCompleted,
                onVerificationFailed: exception =>
                    session.OnVerificationFailed(FirebaseAuthExceptionFactory.Create(exception)),
                onCodeAutoRetrievalTimeOut: session.OnCodeAutoRetrievalTimeOut
            );
            var options = PhoneAuthOptions
                .NewBuilder()
                .SetPhoneNumber(phoneNumber)
                .SetTimeout(Long.ValueOf(60), TimeUnit.Seconds!)
                .SetActivity(activity)
                .SetCallbacks(callbacks)
                .Build();

            PhoneAuthProvider.VerifyPhoneNumber(options);
        });
    }

    public Task<PhoneAuthCredential> GetCredentialAsync(string verificationCode)
    {
        return Task.FromResult(
            _verifications.GetCredential(verificationCode, PhoneAuthProvider.GetCredential)
        );
    }
}