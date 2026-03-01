using JavaThrowable = Java.Lang.Throwable;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.Auth;

internal static class FirebaseAuthExceptionFactory
{
    public static bool IsNativeAuthException(Exception exception)
    {
        return exception is JavaThrowable throwable &&
            GetNativeExceptionTypeName(throwable).StartsWith("com.google.firebase", StringComparison.Ordinal);
    }

    public static CrossPlatformFirebaseAuthException Create(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if(exception is not JavaThrowable throwable) {
            throw new ArgumentException("Expected a native Firebase Auth exception.", nameof(exception));
        }

        return new CrossPlatformFirebaseAuthException(
            FirebaseAuthPlatform.Android,
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
        } catch(Exception exception) when (IsNativeAuthException(exception)) {
            throw Create(exception);
        }
    }

    public static async Task<T> Wrap<T>(Func<Task<T>> operation)
    {
        try {
            return await operation();
        } catch(Exception exception) when (IsNativeAuthException(exception)) {
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

    private static string? GetNativeErrorCode(Exception exception)
    {
        var errorCode = exception
            .GetType()
            .GetProperty("ErrorCode")?
            .GetValue(exception) as string;

        return string.IsNullOrEmpty(errorCode)
            ? null
            : errorCode;
    }
}
