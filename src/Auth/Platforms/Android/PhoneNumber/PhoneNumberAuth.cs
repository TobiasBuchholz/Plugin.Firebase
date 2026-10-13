using Firebase.Auth;
using Java.Lang;
using Java.Util.Concurrent;

namespace Plugin.Firebase.Auth.Platforms.Android.PhoneNumber;

public sealed class PhoneNumberAuth
{
    private string? _verificationId;
    private PhoneAuthCredential? _instantCredential;

    public Task VerifyPhoneNumberAsync(Activity activity, string phoneNumber)
    {
        _verificationId = null;
        _instantCredential = null;
        var completion = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var callbacks = new PhoneVerificationStateChangeCallbacks(
            onCodeSent: x => {
                _verificationId = x.VerificationId;
                completion.TrySetResult();
            },
            onVerificationCompleted: credential => {
                // instant verification: no code is sent, but the credential is already usable
                _instantCredential = credential;
                completion.TrySetResult();
            },
            onVerificationFailed: exception => completion.TrySetException(exception),
            onCodeAutoRetrievalTimeOut: verificationId => {
                // auto retrieval timed out, but the verification ID stays usable for manual entry
                _verificationId = verificationId;
                completion.TrySetResult();
            }
        );
        var options = PhoneAuthOptions
            .NewBuilder()
            .SetPhoneNumber(phoneNumber)
            .SetTimeout(Long.ValueOf(60), TimeUnit.Seconds!)
            .SetActivity(activity)
            .SetCallbacks(callbacks)
            .Build();

        PhoneAuthProvider.VerifyPhoneNumber(options);
        return completion.Task;
    }

    public Task<PhoneAuthCredential> GetCredentialAsync(string verificationCode)
    {
        if(_instantCredential is not null) {
            // instant verification already produced a credential; no code is needed
            return Task.FromResult(_instantCredential);
        }
        if(_verificationId is null) {
            throw new InvalidOperationException(
                "VerifyPhoneNumberAsync must be called before GetCredentialAsync."
            );
        }
        return Task.FromResult(PhoneAuthProvider.GetCredential(_verificationId, verificationCode));
    }
}