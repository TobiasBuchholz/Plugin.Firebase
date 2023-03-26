using Firebase.Auth;
using FirebaseAuth = Firebase.Auth.Auth;

namespace Plugin.Firebase.Auth.Platforms.iOS.Email
{
    public sealed class EmailAuth
    {
        public Task<AuthCredential> GetCredentialAsync(string email, string password)
        {
            return Task.FromResult(EmailAuthProvider.GetCredentialFromPassword(email, password));
        }

        public Task CreateUserAsync(string email, string password)
        {
            return FirebaseAuth.DefaultInstance.CreateUserAsync(email, password);
        }
    }
}