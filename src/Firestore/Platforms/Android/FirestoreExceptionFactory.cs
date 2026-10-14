using Firebase.Firestore;
using Plugin.Firebase.Core.Exceptions;
using JavaThrowable = Java.Lang.Throwable;
using NativeFirebaseException = Firebase.FirebaseException;

namespace Plugin.Firebase.Firestore.Platforms.Android;

/// <summary>
/// Wraps native Firebase failures in <see cref="CrossPlatformFirebaseFirestoreException"/>. The type name
/// is the Java canonical class name, the error code is the <see cref="FirebaseFirestoreException.Code"/>
/// name, and the error domain is <see langword="null"/>.
/// </summary>
internal static class FirestoreExceptionFactory
{
    public static bool IsNativeFirestoreException(Exception exception)
    {
        return exception is NativeFirebaseException;
    }

    public static CrossPlatformFirebaseFirestoreException Create(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if(exception is not NativeFirebaseException nativeException) {
            throw new ArgumentException("Expected a native Firebase exception.", nameof(exception));
        }

        return new CrossPlatformFirebaseFirestoreException(
            GetNativeExceptionTypeName(nativeException),
            nativeException.Message,
            nativeException,
            nativeErrorCode: (nativeException as FirebaseFirestoreException)?.GetCode()?.Name()
        );
    }

    public static async Task Wrap(Func<Task> operation)
    {
        try {
            await operation();
        } catch(Exception exception) when(IsNativeFirestoreException(exception)) {
            throw Create(exception);
        }
    }

    public static async Task<T> Wrap<T>(Func<Task<T>> operation)
    {
        try {
            return await operation();
        } catch(Exception exception) when(IsNativeFirestoreException(exception)) {
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
}