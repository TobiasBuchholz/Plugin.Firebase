namespace Plugin.Firebase.Auth.Facebook;

public interface IFirebaseAuthFacebook : IDisposable
{
    /// <summary>
    /// Signs in using a facebook account.
    /// </summary>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> SignInWithFacebookAsync();

    /// <summary>
    /// Link the signed in user with a facebook account.
    /// </summary>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> LinkWithFacebookAsync();

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    Task SignOutAsync();
}