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
        var ex = Assert.Throws<ArgumentNullException>(
            () => new CrossPlatformFirebaseException(null!, "message", new Exception("inner"))
        );

        Assert.Equal("nativeExceptionTypeName", ex.ParamName);
    }

    [Fact]
    public void Ctor_NullInnerException_Throws()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => new CrossPlatformFirebaseException("type", "message", null!)
        );

        Assert.Equal("innerException", ex.ParamName);
    }

    [Fact]
    public void Ctor_NullInnerExceptionWithoutNativeMessage_ThrowsArgumentNullException()
    {
        // The message falls back to the inner exception's message, which must not be read before the null guard.
        var ex = Assert.Throws<ArgumentNullException>(
            () => new CrossPlatformFirebaseException("type", nativeErrorMessage: null, null!)
        );

        Assert.Equal("innerException", ex.ParamName);
    }

    [Fact]
    public void ServiceExceptions_NullInnerExceptionWithoutNativeMessage_ThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new CrossPlatformFirebaseAuthException("type", nativeErrorMessage: null, null!)
        );
        Assert.Throws<ArgumentNullException>(
            () => new CrossPlatformFirebaseFirestoreException("type", nativeErrorMessage: null, null!)
        );
        Assert.Throws<ArgumentNullException>(
            () => new CrossPlatformFirebaseStorageException("type", nativeErrorMessage: null, null!)
        );
    }

    [Fact]
    public void FirestoreCtor_PassesNativeMetadataThrough()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new CrossPlatformFirebaseFirestoreException(
            "NSError",
            "native message",
            inner,
            nativeErrorDomain: "FIRFirestoreErrorDomain",
            nativeErrorCode: "InvalidArgument"
        );

        Assert.Equal("NSError", ex.NativeExceptionTypeName);
        Assert.Equal("FIRFirestoreErrorDomain", ex.NativeErrorDomain);
        Assert.Equal("InvalidArgument", ex.NativeErrorCode);
        Assert.Equal("native message", ex.NativeErrorMessage);
        Assert.Equal("native message", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void FirestoreCtor_WithoutDomainOrCode_LeavesThemNull()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new CrossPlatformFirebaseFirestoreException(
            "com.google.firebase.firestore.FirebaseFirestoreException",
            nativeErrorMessage: null,
            inner
        );

        Assert.Equal("com.google.firebase.firestore.FirebaseFirestoreException", ex.NativeExceptionTypeName);
        Assert.Null(ex.NativeErrorDomain);
        Assert.Null(ex.NativeErrorCode);
        Assert.Equal("inner", ex.NativeErrorMessage);
        Assert.Equal("inner", ex.Message);
    }

    [Fact]
    public void StorageCtor_PassesNativeMetadataThrough()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new CrossPlatformFirebaseStorageException(
            "com.google.firebase.storage.StorageException",
            "Object does not exist at location.",
            inner,
            nativeErrorCode: "-13010",
            nativeHttpResultCode: 404
        );

        Assert.Equal("com.google.firebase.storage.StorageException", ex.NativeExceptionTypeName);
        Assert.Null(ex.NativeErrorDomain);
        Assert.Equal("-13010", ex.NativeErrorCode);
        Assert.Equal(404, ex.NativeHttpResultCode);
        Assert.Equal("Object does not exist at location.", ex.NativeErrorMessage);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void StorageCtor_WithoutHttpResultCode_LeavesItNull()
    {
        var ex = new CrossPlatformFirebaseStorageException("NSError", "message", new Exception("inner"));

        Assert.Null(ex.NativeHttpResultCode);
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