namespace Plugin.Firebase.Auth.Google;

public interface IFirebaseAuthGoogle : IDisposable
{
    /// <summary>
    /// Signs in using a google account.
    /// </summary>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> SignInWithGoogleAsync();
    
    /// <summary>
    /// Link the signed in user with a google account.
    /// </summary>
    /// <returns>The signed in <c>IFirebaseUser</c> object.</returns>
    Task<IFirebaseUser> LinkWithGoogleAsync();
    
    /// <summary>
    /// Signs out the current user.
    /// </summary>
    Task SignOutAsync();
}