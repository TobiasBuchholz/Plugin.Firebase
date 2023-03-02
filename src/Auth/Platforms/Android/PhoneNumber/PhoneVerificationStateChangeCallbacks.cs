using Firebase;
using Firebase.Auth;

namespace Plugin.Firebase.Auth.Platforms.Android.PhoneNumber;

public sealed class PhoneVerificationStateChangeCallbacks : PhoneAuthProvider.OnVerificationStateChangedCallbacks
{
    private readonly Action<(string, PhoneAuthProvider.ForceResendingToken)> _onCodeSent;
    private readonly Action<PhoneAuthCredential> _onVerificationCompleted;
    private readonly Action<FirebaseException> _onVerificationFailed;
    private readonly Action<string> _onCodeAutoRetrievalTimeOut;

    public PhoneVerificationStateChangeCallbacks(
        Action<(string VerificationId, PhoneAuthProvider.ForceResendingToken forceResendingToken)> onCodeSent = null,
        Action<PhoneAuthCredential> onVerificationCompleted = null,
        Action<FirebaseException> onVerificationFailed = null,
        Action<string> onCodeAutoRetrievalTimeOut = null)
    {
        _onCodeSent = onCodeSent;
        _onVerificationCompleted = onVerificationCompleted;
        _onVerificationFailed = onVerificationFailed;
        _onCodeAutoRetrievalTimeOut = onCodeAutoRetrievalTimeOut;
    }

    public override void OnVerificationCompleted(PhoneAuthCredential credential)
    {
        _onVerificationCompleted?.Invoke(credential);
    }

    public override void OnVerificationFailed(FirebaseException exception)
    {
        _onVerificationFailed?.Invoke(exception);
    }

    public override void OnCodeSent(string verificationId, PhoneAuthProvider.ForceResendingToken forceResendingToken)
    {
        base.OnCodeSent(verificationId, forceResendingToken);
        _onCodeSent?.Invoke((verificationId, forceResendingToken));
    }

    public override void OnCodeAutoRetrievalTimeOut(string verificationId)
    {
        base.OnCodeAutoRetrievalTimeOut(verificationId);
        _onCodeAutoRetrievalTimeOut?.Invoke(verificationId);
    }
}