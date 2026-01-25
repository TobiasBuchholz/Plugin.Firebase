using Foundation;
using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.Auth.Platforms.iOS;

internal static class FirebaseAuthExceptionFactory
{
    private const string ErrorNameKey = "FIRAuthErrorUserInfoNameKey";
    private const string ErrorEmailKey = "FIRAuthErrorUserInfoEmailKey";

    public static FirebaseAuthException Create(NSErrorException exception)
    {
        var error = exception.Error;
        var errorCode = TryGetErrorName(error) ?? GetAuthErrorCodeName(error);
        var email = TryGetEmail(error);

        return FirebaseAuthException.FromErrorCode(
            errorCode,
            error.LocalizedDescription,
            exception,
            error.Domain,
            error.Code,
            email);
    }

    private static string? GetAuthErrorCodeName(NSError error)
    {
        var code = IntPtr.Size == 8
            ? (global::Firebase.Auth.AuthErrorCode) (nint) error.Code
            : (global::Firebase.Auth.AuthErrorCode) (int) (nint) error.Code;

        return code.ToString();
    }

    private static string? TryGetErrorName(NSError error)
    {
        if(error.UserInfo is null) {
            return null;
        }

        return error.UserInfo[new NSString(ErrorNameKey)]?.ToString();
    }

    private static string? TryGetEmail(NSError error)
    {
        if(error.UserInfo is null) {
            return null;
        }

        return error.UserInfo[new NSString(ErrorEmailKey)]?.ToString();
    }
}