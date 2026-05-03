using Firebase.Auth;

namespace Plugin.Firebase.Auth;

public partial interface IFirebaseAuth
{
    /// <summary>
    /// Signs in using a native Firebase Auth credential.
    /// </summary>
    /// <param name="credential">The native iOS Firebase Auth credential.</param>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> SignInWithCredentialAsync(AuthCredential credential);

    /// <summary>
    /// Links the signed in user with a native Firebase Auth credential.
    /// </summary>
    /// <param name="credential">The native iOS Firebase Auth credential.</param>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> LinkWithCredentialAsync(AuthCredential credential);
}