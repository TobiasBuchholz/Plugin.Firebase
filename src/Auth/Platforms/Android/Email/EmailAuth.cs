using Firebase.Auth;

namespace Plugin.Firebase.Auth.Platforms.Android.Email;

public sealed class EmailAuth
{
    public Task<AuthCredential> GetCredentialAsync(string email, string password)
    {
        return Task.FromResult(EmailAuthProvider.GetCredential(email, password));
    }

    public async Task<IFirebaseUser> CreateUserAsync(string email, string password)
    {
        var result = await FirebaseAuth.Instance.CreateUserWithEmailAndPasswordAsync(email, password);
        return new FirebaseUserWrapper(result.User);
    }
}