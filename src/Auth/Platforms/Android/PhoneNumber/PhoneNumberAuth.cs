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
        // each request gets its own callbacks bound to its attempt, so stale callbacks can't touch a newer attempt
        var attempt = _verifications.Start(phoneNumber);
        var callbacks = new PhoneVerificationStateChangeCallbacks(
            onCodeSent: x => attempt.OnCodeSent(x.VerificationId),
            onVerificationCompleted: attempt.OnVerificationCompleted,
            onVerificationFailed: exception =>
                attempt.OnVerificationFailed(FirebaseAuthExceptionFactory.Create(exception)),
            onCodeAutoRetrievalTimeOut: attempt.OnCodeAutoRetrievalTimeOut
        );
        var options = PhoneAuthOptions
            .NewBuilder()
            .SetPhoneNumber(phoneNumber)
            .SetTimeout(Long.ValueOf(60), TimeUnit.Seconds!)
            .SetActivity(activity)
            .SetCallbacks(callbacks)
            .Build();

        PhoneAuthProvider.VerifyPhoneNumber(options);
        return attempt.Task;
    }

    public Task<PhoneAuthCredential> GetCredentialAsync(string verificationCode)
    {
        return Task.FromResult(
            _verifications.GetCredential(verificationCode, PhoneAuthProvider.GetCredential)
        );
    }
}