using Plugin.Firebase.Auth;

namespace Plugin.Firebase.IntegrationTests;

internal sealed class AuthTestUserScope : IAsyncDisposable
{
    private readonly IFirebaseAuth _auth;
    private readonly string? _email;
    private readonly string? _password;
    private readonly bool _deleteOnDispose;
    private IFirebaseUser? _user;

    private AuthTestUserScope(
        IFirebaseAuth auth,
        IFirebaseUser user,
        string? email,
        string? password,
        bool deleteOnDispose)
    {
        _auth = auth;
        _user = user;
        _email = email;
        _password = password;
        _deleteOnDispose = deleteOnDispose;
    }

    public IFirebaseUser User =>
        _user ?? throw new ObjectDisposedException(nameof(AuthTestUserScope));

    public string? Email => _email;

    public static async Task<AuthTestUserScope> CreateWithEmailAndPasswordAsync(
        IFirebaseAuth auth,
        string email,
        string password = IntegrationTestUsers.DefaultPassword,
        bool deleteOnDispose = true)
    {
        await auth.CreateUserAsync(email, password);
        var user = auth.CurrentUser ?? throw new InvalidOperationException("Expected created test user to be current.");
        return new AuthTestUserScope(auth, user, email, password, deleteOnDispose);
    }

    public static async Task<AuthTestUserScope> SignInWithEmailAndPasswordAsync(
        IFirebaseAuth auth,
        string email,
        string password = IntegrationTestUsers.DefaultPassword,
        bool createsUserAutomatically = true,
        bool deleteOnDispose = true)
    {
        var user = await auth.SignInWithEmailAndPasswordAsync(
            email,
            password,
            createsUserAutomatically);
        return new AuthTestUserScope(auth, user, email, password, deleteOnDispose);
    }

    public static Task<AuthTestUserScope> SignInWithUniqueEmailAndPasswordAsync(
        IFirebaseAuth auth,
        string prefix,
        string password = IntegrationTestUsers.DefaultPassword,
        bool deleteOnDispose = true)
    {
        return SignInWithEmailAndPasswordAsync(
            auth,
            IntegrationTestData.UniqueEmail(prefix),
            password,
            deleteOnDispose: deleteOnDispose);
    }

    public static async Task<AuthTestUserScope> SignInAnonymouslyAsync(
        IFirebaseAuth auth,
        bool deleteOnDispose = true)
    {
        var user = await auth.SignInAnonymouslyAsync();
        return new AuthTestUserScope(auth, user, null, null, deleteOnDispose);
    }

    public static AuthTestUserScope TrackCurrentUser(
        IFirebaseAuth auth,
        string? email = null,
        string? password = null,
        bool deleteOnDispose = true)
    {
        var user = auth.CurrentUser ?? throw new InvalidOperationException("Expected a current test user to track.");
        return new AuthTestUserScope(auth, user, email ?? user.Email, password, deleteOnDispose);
    }

    public async ValueTask DisposeAsync()
    {
        try {
            if(_deleteOnDispose && _user != null) {
                await EnsureUserIsCurrentAsync();
                if(_auth.CurrentUser?.Uid == _user.Uid) {
                    await _auth.CurrentUser.DeleteAsync();
                }
            }
        } catch(Exception e) {
            TestLog.Write($"[AUTH CLEANUP ERROR] {_email ?? _user?.Uid ?? "unknown"}: {e}");
        }
        finally {
            _user = null;
            await _auth.SignOutAsync();
        }
    }

    private async Task EnsureUserIsCurrentAsync()
    {
        if(_user == null || _auth.CurrentUser?.Uid == _user.Uid) {
            return;
        }

        if(!string.IsNullOrWhiteSpace(_email) && !string.IsNullOrWhiteSpace(_password)) {
            _user = await _auth.SignInWithEmailAndPasswordAsync(
                _email,
                _password,
                createsUserAutomatically: false);
        }
    }
}