using Firebase.Auth;
using Google.SignIn;
using Plugin.Firebase.Auth.Platforms.iOS.Extensions;
using Plugin.Firebase.Auth.Platforms.iOS.Google;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using FirebaseAuth = Firebase.Auth.Auth;

namespace Plugin.Firebase.Auth.Google;

public sealed class FirebaseAuthGoogleImplementation : DisposableBase, IFirebaseAuthGoogle
{
    public static void Initialize()
    {
        var googleServiceDictionary = NSMutableDictionary.FromFile("GoogleService-Info.plist");
        var clientId = googleServiceDictionary["CLIENT_ID"].ToString();
        SignIn.SharedInstance.Configuration = new Configuration(clientId);
    }

    public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    {
        return SignIn.SharedInstance.HandleUrl(url);
    }

    private readonly FirebaseAuth _firebaseAuth;
    private static Lazy<GoogleAuth> _googleAuth;

    public FirebaseAuthGoogleImplementation()
    {
        _firebaseAuth = FirebaseAuth.DefaultInstance;
        _googleAuth = new Lazy<GoogleAuth>(() => new GoogleAuth());
    }

    public async Task<IFirebaseUser> SignInWithGoogleAsync()
    {
        try {
            var credential = await _googleAuth.Value.GetCredentialAsync(ViewController);
            return await SignInWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private async Task<IFirebaseUser> SignInWithCredentialAsync(AuthCredential credential)
    {
        var authResult = await _firebaseAuth.SignInWithCredentialAsync(credential);
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    private static FirebaseAuthException GetFirebaseAuthException(NSErrorException ex)
    {
        AuthErrorCode errorCode;
        if(IntPtr.Size == 8) { // 64 bits devices
            errorCode = (AuthErrorCode) (long) ex.Error.Code;
        } else { // 32 bits devices
            errorCode = (AuthErrorCode) (int) ex.Error.Code;
        }

        Enum.TryParse(errorCode.ToString(), out FIRAuthError authError);
        return new FirebaseAuthException(authError, ex.Error.LocalizedDescription);
    }

    public async Task<IFirebaseUser> LinkWithGoogleAsync()
    {
        try {
            var credential = await _googleAuth.Value.GetCredentialAsync(ViewController);
            return await LinkWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            _googleAuth.Value.SignOut();
            throw GetFirebaseAuthException(e);
        }
    }

    private async Task<IFirebaseUser> LinkWithCredentialAsync(AuthCredential credential)
    {
        var authResult = await _firebaseAuth.CurrentUser.LinkAsync(credential);
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public Task SignOutAsync()
    {
        _googleAuth.Value.SignOut();
        return Task.CompletedTask;
    }

    private static UIViewController ViewController {
        get {
            var rootViewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            if(rootViewController == null) {
                throw new NullReferenceException("RootViewController is null");
            }
            return rootViewController.PresentedViewController ?? rootViewController;
        }
    }
}