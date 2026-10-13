using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.UnitTests.Exceptions;

public class CrossPlatformFirebaseExceptionTests
{
    [Fact]
    public void Ctor_SetsNativeMetadataAndInnerException()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new CrossPlatformFirebaseException(
            "com.example.NativeException",
            "native message",
            inner,
            nativeErrorDomain: "NativeDomain",
            nativeErrorCode: "42"
        );

        Assert.Equal("com.example.NativeException", ex.NativeExceptionTypeName);
        Assert.Equal("NativeDomain", ex.NativeErrorDomain);
        Assert.Equal("42", ex.NativeErrorCode);
        Assert.Equal("native message", ex.NativeErrorMessage);
        Assert.Equal("native message", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Ctor_WithoutNativeMessage_FallsBackToInnerExceptionMessage()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new CrossPlatformFirebaseException("NSError", nativeErrorMessage: null, inner);

        Assert.Equal("inner", ex.NativeErrorMessage);
        Assert.Equal("inner", ex.Message);
        Assert.Null(ex.NativeErrorDomain);
        Assert.Null(ex.NativeErrorCode);
    }

    [Fact]
    public void Ctor_NullTypeName_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CrossPlatformFirebaseException(null!, "message", new Exception("inner"))
        );
    }

    [Fact]
    public void Ctor_NullInnerException_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CrossPlatformFirebaseException("type", "message", null!)
        );
    }

    [Fact]
    public void ServiceExceptions_AreCrossPlatformFirebaseExceptions()
    {
        var inner = new Exception("inner");

        var auth = new CrossPlatformFirebaseAuthException("type", "message", inner);
        var firestore = new CrossPlatformFirebaseFirestoreException("type", "message", inner);
        var storage = new CrossPlatformFirebaseStorageException("type", "message", inner);

        Assert.IsAssignableFrom<CrossPlatformFirebaseException>(auth);
        Assert.IsAssignableFrom<CrossPlatformFirebaseException>(firestore);
        Assert.IsAssignableFrom<CrossPlatformFirebaseException>(storage);
        Assert.IsAssignableFrom<FirebaseException>(firestore);
        Assert.IsAssignableFrom<FirebaseException>(storage);
    }
}