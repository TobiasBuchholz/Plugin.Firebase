using System.Globalization;
using Firebase.Auth;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.Auth;

internal static class FirebaseAuthExceptionFactory
{
    public static bool IsNativeAuthException(Exception exception)
    {
        return exception is NSErrorException;
    }

    public static CrossPlatformFirebaseAuthException Create(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if(exception is not NSErrorException nsErrorException) {
            throw new ArgumentException("Expected a native Firebase Auth exception.", nameof(exception));
        }

        return new CrossPlatformFirebaseAuthException(
            nameof(NSError),
            nsErrorException.Error.LocalizedDescription ?? nsErrorException.Message,
            nsErrorException,
            nativeErrorDomain: nsErrorException.Error.Domain,
            nativeErrorCode: GetNativeErrorCode(nsErrorException.Error)
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

    private static string GetNativeErrorCode(NSError error)
    {
        var authErrorCode = ToAuthErrorCode(error.Code);

        return Enum.IsDefined(authErrorCode)
            ? authErrorCode.ToString()
            : ((long) error.Code).ToString(CultureInfo.InvariantCulture);
    }

    private static AuthErrorCode ToAuthErrorCode(nint errorCode)
    {
        return IntPtr.Size == 8
            ? (AuthErrorCode) (long) errorCode
            : (AuthErrorCode) (int) errorCode;
    }
}
