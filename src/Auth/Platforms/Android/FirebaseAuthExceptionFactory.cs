using JavaThrowable = Java.Lang.Throwable;
using NativeFirebaseAuthException = Firebase.Auth.FirebaseAuthException;
using NativeFirebaseException = Firebase.FirebaseException;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.Auth;

internal static class FirebaseAuthExceptionFactory
{
    public static bool IsNativeAuthException(Exception exception)
    {
        return exception is NativeFirebaseException;
    }

    public static CrossPlatformFirebaseAuthException Create(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if(exception is not JavaThrowable throwable) {
            throw new ArgumentException("Expected a native Firebase Auth exception.", nameof(exception));
        }

        return new CrossPlatformFirebaseAuthException(
            GetNativeExceptionTypeName(throwable),
            exception.Message,
            exception,
            nativeErrorCode: GetNativeErrorCode(exception)
        );
    }

    public static async Task Wrap(Func<Task> operation)
    {
        try {
            await operation();
        } catch(Exception exception) when(IsNativeAuthException(exception)) {
            throw Create(exception);
        }
    }

    public static async Task<T> Wrap<T>(Func<Task<T>> operation)
    {
        try {
            return await operation();
        } catch(Exception exception) when(IsNativeAuthException(exception)) {
            throw Create(exception);
        }
    }

    private static string GetNativeExceptionTypeName(JavaThrowable throwable)
    {
        var canonicalName = throwable.Class?.CanonicalName;
        return string.IsNullOrEmpty(canonicalName)
            ? throwable.GetType().FullName ?? throwable.GetType().Name
            : canonicalName;
    }

    // A static reference, not reflection: it keeps FirebaseAuthException, its ErrorCode getter and
    // its Java type mapping in trimmed (Release) builds. Every Auth exception subclass then surfaces
    // as at least this type, even when the subclass binding is trimmed away. Reflection found no
    // ErrorCode property in trimmed builds, so the code was always null there.
    private static string? GetNativeErrorCode(Exception exception)
    {
        var errorCode = (exception as NativeFirebaseAuthException)?.ErrorCode;

        return string.IsNullOrEmpty(errorCode)
            ? null
            : errorCode;
    }
}