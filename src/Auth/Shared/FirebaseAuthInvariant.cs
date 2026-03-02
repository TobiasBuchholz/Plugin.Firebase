using Plugin.Firebase.Core.Exceptions;

namespace Plugin.Firebase.Auth;

internal static class FirebaseAuthInvariant
{
    public static T EnsureNotNull<T>(T? value, string message) where T : class
    {
        return value ?? throw new FirebaseException(message);
    }
}
