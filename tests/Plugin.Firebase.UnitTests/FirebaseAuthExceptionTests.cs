using Plugin.Firebase.Core.Exceptions;
using Xunit;

namespace Plugin.Firebase.UnitTests;

public sealed class FirebaseAuthExceptionTests
{
    public static IEnumerable<object[]> KnownCodes =>
        new[]
        {
            new object[] { "ERROR_INVALID_EMAIL", FIRAuthError.InvalidEmail },
            new object[] { "ERROR_WRONG_PASSWORD", FIRAuthError.WrongPassword },
            new object[] { "ERROR_WEAK_PASSWORD", FIRAuthError.WeakPassword },
            new object[] { "ERROR_EMAIL_ALREADY_IN_USE", FIRAuthError.EmailAlreadyInUse },
            new object[] { "ERROR_USER_NOT_FOUND", FIRAuthError.UserNotFound },
            new object[] { "ERROR_USER_DISABLED", FIRAuthError.UserDisabled },
            new object[] { "ERROR_INVALID_CREDENTIAL", FIRAuthError.InvalidCredential },
            new object[] { "ERROR_ACCOUNT_EXISTS_WITH_DIFFERENT_CREDENTIAL", FIRAuthError.AccountExistsWithDifferentCredential },
            new object[] { "ERROR_CREDENTIAL_ALREADY_IN_USE", FIRAuthError.CredentialAlreadyInUse },
            new object[] { "ERROR_REQUIRES_RECENT_LOGIN", FIRAuthError.RequiresRecentLogin },
            new object[] { "ERROR_OPERATION_NOT_ALLOWED", FIRAuthError.OperationNotAllowed },
            new object[] { "ERROR_INVALID_CUSTOM_TOKEN", FIRAuthError.InvalidCustomToken },
            new object[] { "ERROR_CUSTOM_TOKEN_MISMATCH", FIRAuthError.CustomTokenMismatch },
            new object[] { "ERROR_INVALID_USER_TOKEN", FIRAuthError.InvalidUserToken },
            new object[] { "ERROR_KEYCHAIN_ERROR", FIRAuthError.KeychainError },
            new object[] { "ERROR_INTERNAL_ERROR", FIRAuthError.InternalError },
            new object[] { "ERROR_TOO_MANY_REQUESTS", FIRAuthError.TooManyRequests },
            new object[] { "ERROR_NETWORK_REQUEST_FAILED", FIRAuthError.NetworkError },
            new object[] { "FIRAuthErrorCodeInvalidEmail", FIRAuthError.InvalidEmail },
            new object[] { "AuthErrorCodeInvalidCredential", FIRAuthError.InvalidCredential },
        };

    [Theory]
    [MemberData(nameof(KnownCodes))]
    public void FromErrorCode_maps_known_codes(string code, FIRAuthError expected)
    {
        var exception = FirebaseAuthException.FromErrorCode(code, "message");
        Assert.Equal(expected, exception.Reason);
        Assert.Equal(code, exception.ErrorCode);
    }

    [Fact]
    public void FromErrorCode_returns_undefined_for_unknown_code()
    {
        var exception = FirebaseAuthException.FromErrorCode("ERROR_SOMETHING_UNKNOWN", "message");
        Assert.Equal(FIRAuthError.Undefined, exception.Reason);
    }

    [Fact]
    public void FromErrorCode_preserves_metadata()
    {
        var exception = FirebaseAuthException.FromErrorCode(
            "ERROR_WRONG_PASSWORD",
            "bad password",
            email: "user@test.com",
            nativeErrorDomain: "domain",
            nativeErrorCode: 123);

        Assert.Equal("user@test.com", exception.Email);
        Assert.Equal("domain", exception.NativeErrorDomain);
        Assert.Equal(123, exception.NativeErrorCode);
    }
}
