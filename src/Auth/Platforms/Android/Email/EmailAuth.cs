using Firebase.Auth;

namespace Plugin.Firebase.Auth.Platforms.Android.Email;

public sealed class EmailAuth
{
    public Task<AuthCredential> GetCredentialAsync(string email, string password)
    {
        return Task.FromResult(EmailAuthProvider.GetCredential(email, password));
    }

    public Task CreateUserAsync(string email, string password)
    {
        return FirebaseAuth.Instance.CreateUserWithEmailAndPasswordAsync(email, password);
    }
}