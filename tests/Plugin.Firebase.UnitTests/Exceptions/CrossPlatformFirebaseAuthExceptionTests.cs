using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.UnitTests.Exceptions;

public class CrossPlatformFirebaseAuthExceptionTests
{
    [Fact]
    public void Ctor_SetsNativeMetadataAndInnerException()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new CrossPlatformFirebaseAuthException(
            FirebaseAuthAndroidExceptionTypeNames.InvalidCredentialsException,
            "Bad credentials",
            inner,
            nativeErrorCode: "ERROR_WRONG_PASSWORD"
        );

        Assert.Equal(
            FirebaseAuthAndroidExceptionTypeNames.InvalidCredentialsException,
            ex.NativeExceptionTypeName
        );
        Assert.Equal("ERROR_WRONG_PASSWORD", ex.NativeErrorCode);
        Assert.Equal("Bad credentials", ex.NativeErrorMessage);
        Assert.Equal("Bad credentials", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Ctor_WithoutNativeMessage_FallsBackToInnerExceptionMessage()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new CrossPlatformFirebaseAuthException(
            "NSError",
            nativeErrorMessage: null,
            inner,
            nativeErrorDomain: "FIRAuthErrorDomain",
            nativeErrorCode: "UserNotFound"
        );

        Assert.Equal("inner", ex.NativeErrorMessage);
        Assert.Equal("inner", ex.Message);
        Assert.Equal("FIRAuthErrorDomain", ex.NativeErrorDomain);
    }

    [Theory]
    [InlineData(FirebaseAuthAndroidExceptionTypeNames.InvalidCredentialsException, FirebaseAuthFailure.InvalidCredentials)]
    [InlineData(FirebaseAuthAndroidExceptionTypeNames.InvalidUserException, FirebaseAuthFailure.UserNotFound)]
    [InlineData(FirebaseAuthAndroidExceptionTypeNames.UserCollisionException, FirebaseAuthFailure.UserCollision)]
    [InlineData(FirebaseAuthAndroidExceptionTypeNames.WeakPasswordException, FirebaseAuthFailure.WeakPassword)]
    [InlineData(FirebaseAuthAndroidExceptionTypeNames.RecentLoginRequiredException, FirebaseAuthFailure.RequiresRecentLogin)]
    [InlineData("com.google.firebase.FirebaseTooManyRequestsException", FirebaseAuthFailure.TooManyRequests)]
    public void TryClassify_AndroidCommonFailures_ReturnsExpectedFailure(
        string nativeExceptionTypeName,
        FirebaseAuthFailure expectedFailure
    )
    {
        var ex = new CrossPlatformFirebaseAuthException(
            nativeExceptionTypeName,
            "message",
            new Exception("inner")
        );

        var failure = FirebaseAuthErrorClassifier.TryClassify(ex);

        Assert.Equal(expectedFailure, failure);
    }

    [Theory]
    [InlineData("InvalidCredential", FirebaseAuthFailure.InvalidCredentials)]
    [InlineData("UserNotFound", FirebaseAuthFailure.UserNotFound)]
    [InlineData("EmailAlreadyInUse", FirebaseAuthFailure.UserCollision)]
    [InlineData("AccountExistsWithDifferentCredential", FirebaseAuthFailure.UserCollision)]
    [InlineData("WeakPassword", FirebaseAuthFailure.WeakPassword)]
    [InlineData("RequiresRecentLogin", FirebaseAuthFailure.RequiresRecentLogin)]
    [InlineData("TooManyRequests", FirebaseAuthFailure.TooManyRequests)]
    public void TryClassify_iOSCommonFailures_ReturnsExpectedFailure(
        string nativeErrorCode,
        FirebaseAuthFailure expectedFailure
    )
    {
        var ex = new CrossPlatformFirebaseAuthException(
            "NSError",
            "message",
            new Exception("inner"),
            nativeErrorDomain: "FIRAuthErrorDomain",
            nativeErrorCode: nativeErrorCode
        );

        var failure = FirebaseAuthErrorClassifier.TryClassify(ex);

        Assert.Equal(expectedFailure, failure);
    }

    [Fact]
    public void TryClassify_UnknownFailure_ReturnsNull()
    {
        var ex = new CrossPlatformFirebaseAuthException(
            "com.google.firebase.auth.SomeNewException",
            "message",
            new Exception("inner")
        );

        var failure = FirebaseAuthErrorClassifier.TryClassify(ex);

        Assert.Null(failure);
    }

    [Fact]
    public void TryClassify_AndroidExceptionWithDomain_PrefersNativeDomainDispatch()
    {
        var ex = new CrossPlatformFirebaseAuthException(
            FirebaseAuthAndroidExceptionTypeNames.InvalidCredentialsException,
            "message",
            new Exception("inner"),
            nativeErrorDomain: "FIRAuthErrorDomain",
            nativeErrorCode: "WeakPassword"
        );

        var failure = FirebaseAuthErrorClassifier.TryClassify(ex);

        Assert.Equal(FirebaseAuthFailure.WeakPassword, failure);
    }
}