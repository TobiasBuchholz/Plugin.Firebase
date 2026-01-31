using Firebase.Auth;
using FirebaseAuth = Firebase.Auth.Auth;

namespace Plugin.Firebase.Auth.Platforms.iOS.Email
{
    /// <summary>
    /// Provides email/password authentication functionality for iOS using Firebase Auth.
    /// </summary>
    public sealed class EmailAuth
    {
        /// <summary>
        /// Gets a Firebase credential for email/password authentication.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        /// <returns>A task containing the authentication credential.</returns>
        public Task<AuthCredential> GetCredentialAsync(string email, string password)
        {
            return Task.FromResult(EmailAuthProvider.GetCredentialFromPassword(email, password));
        }

        /// <summary>
        /// Creates a new user account with the specified email and password.
        /// </summary>
        /// <param name="email">The email address for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <returns>A task containing the created Firebase user.</returns>
        public async Task<IFirebaseUser> CreateUserAsync(string email, string password)
        {
            var result = await FirebaseAuth.DefaultInstance.CreateUserAsync(email, password);
            return new FirebaseUserWrapper(result.User);
        }
    }
}