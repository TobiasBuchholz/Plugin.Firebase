using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.IntegrationTests.Firestore;

internal static class FirestoreErrorAssertions
{
    public static CrossPlatformFirebaseFirestoreException InvalidArgumentFailure(Exception? exception)
    {
        var firestoreException = Assert.IsType<CrossPlatformFirebaseFirestoreException>(exception);
        if(OperatingSystem.IsAndroid()) {
            Assert.Equal(
                "com.google.firebase.firestore.FirebaseFirestoreException",
                firestoreException.NativeExceptionTypeName);
            Assert.Null(firestoreException.NativeErrorDomain);
            Assert.Equal("INVALID_ARGUMENT", firestoreException.NativeErrorCode);
            Assert.Equal(
                "Firebase.Firestore.FirebaseFirestoreException",
                firestoreException.InnerException?.GetType().FullName);
        } else {
            Assert.Equal("NSError", firestoreException.NativeExceptionTypeName);
            Assert.Equal("FIRFirestoreErrorDomain", firestoreException.NativeErrorDomain);
            Assert.Equal("InvalidArgument", firestoreException.NativeErrorCode);
            Assert.Equal("Foundation.NSErrorException", firestoreException.InnerException?.GetType().FullName);
        }
        Assert.False(string.IsNullOrEmpty(firestoreException.NativeErrorMessage));
        return firestoreException;
    }
}