using Firebase.Auth;
using CrossFirebaseAuthException = Plugin.Firebase.Core.Exceptions.FirebaseAuthException;
using FirebaseAuthExceptionNative = Firebase.Auth.FirebaseAuthException;

namespace Plugin.Firebase.Auth.Platforms.Android;

internal static class FirebaseAuthExceptionFactory
{
    public static CrossFirebaseAuthException Create(Exception exception)
    {
        if(exception is CrossFirebaseAuthException crossException) {
            return crossException;
        }

        if(exception is FirebaseAuthExceptionNative firebaseException) {
            var email = (exception as FirebaseAuthUserCollisionException)?.Email;
            return CrossFirebaseAuthException.FromErrorCode(
                firebaseException.ErrorCode,
                firebaseException.Message,
                exception,
                email: email);
        }

        return CrossFirebaseAuthException.FromErrorCode(
            errorCode: null,
            message: exception.Message,
            inner: exception);
    }
}