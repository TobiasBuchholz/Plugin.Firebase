using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.IntegrationTests.Firestore;

internal static class FirestoreErrorAssertions
{
    public static CrossPlatformFirebaseFirestoreException InvalidArgumentFailure(Exception? exception)
    {
        return NativeFailure(exception, androidCode: "INVALID_ARGUMENT", iosCode: "InvalidArgument");
    }

    public static CrossPlatformFirebaseFirestoreException UnavailableFailure(Exception? exception)
    {
        return NativeFailure(exception, androidCode: "UNAVAILABLE", iosCode: "Unavailable");
    }

    private static CrossPlatformFirebaseFirestoreException NativeFailure(
        Exception? exception,
        string androidCode,
        string iosCode)
    {
        var firestoreException = Assert.IsType<CrossPlatformFirebaseFirestoreException>(exception);
        if(OperatingSystem.IsAndroid()) {
            Assert.Equal(
                "com.google.firebase.firestore.FirebaseFirestoreException",
                firestoreException.NativeExceptionTypeName);
            Assert.Null(firestoreException.NativeErrorDomain);
            Assert.Equal(androidCode, firestoreException.NativeErrorCode);
            Assert.Equal(
                "Firebase.Firestore.FirebaseFirestoreException",
                firestoreException.InnerException?.GetType().FullName);
        } else {
            Assert.Equal("NSError", firestoreException.NativeExceptionTypeName);
            Assert.Equal("FIRFirestoreErrorDomain", firestoreException.NativeErrorDomain);
            Assert.Equal(iosCode, firestoreException.NativeErrorCode);
            Assert.Equal("Foundation.NSErrorException", firestoreException.InnerException?.GetType().FullName);
        }
        Assert.False(string.IsNullOrEmpty(firestoreException.NativeErrorMessage));
        return firestoreException;
    }
}