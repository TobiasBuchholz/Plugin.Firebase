using System.Globalization;
using Firebase.CloudFirestore;
using Foundation;
using Plugin.Firebase.Core.Exceptions;
using FBFirestore = Firebase.CloudFirestore.Firestore;

namespace Plugin.Firebase.Firestore.Platforms.iOS;

/// <summary>
/// Wraps native Firebase failures in <see cref="CrossPlatformFirebaseFirestoreException"/>. The type name
/// is <c>NSError</c>, the error domain is the <see cref="NSError"/> domain, and the error code is the
/// <see cref="FirestoreErrorCode"/> name for a Firestore-domain error with a named code, otherwise the
/// numeric code formatted with the invariant culture.
/// </summary>
internal static class FirestoreExceptionFactory
{
    public static bool IsNativeFirestoreException(Exception exception)
    {
        return exception is NSErrorException;
    }

    public static CrossPlatformFirebaseFirestoreException Create(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if(exception is not NSErrorException nsErrorException) {
            throw new ArgumentException("Expected a native Firebase exception.", nameof(exception));
        }

        return new CrossPlatformFirebaseFirestoreException(
            nameof(NSError),
            nsErrorException.Error.LocalizedDescription ?? nsErrorException.Message,
            nsErrorException,
            nativeErrorDomain: nsErrorException.Error.Domain,
            nativeErrorCode: GetNativeErrorCode(nsErrorException.Error)
        );
    }

    public static CrossPlatformFirebaseFirestoreException Create(NSError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return Create(new NSErrorException(error));
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

    /// <summary>
    /// Returns the Firestore-domain <see cref="NSError"/> that <paramref name="exception"/> was created for by
    /// <see cref="Create(Exception)"/>, or <see langword="null"/>. Only <paramref name="exception"/> itself is
    /// inspected, not its inner exceptions, so an exception that wraps a Firestore failure doesn't match.
    /// </summary>
    public static NSError? GetFirestoreError(Exception exception)
    {
        return exception is CrossPlatformFirebaseFirestoreException { InnerException: NSErrorException { Error: { } error } }
            && IsFirestoreErrorDomain(error)
            ? error
            : null;
    }

    private static string GetNativeErrorCode(NSError error)
    {
        var code = (long) error.Code;
        var firestoreErrorCode = (FirestoreErrorCode) code;

        return IsFirestoreErrorDomain(error) && Enum.IsDefined(firestoreErrorCode)
            ? firestoreErrorCode.ToString()
            : code.ToString(CultureInfo.InvariantCulture);
    }

    private static bool IsFirestoreErrorDomain(NSError error)
    {
        return string.Equals(error.Domain, FBFirestore.ErrorDomain.ToString(), StringComparison.Ordinal);
    }
}