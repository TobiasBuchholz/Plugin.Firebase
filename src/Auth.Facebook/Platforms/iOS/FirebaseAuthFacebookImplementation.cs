using Facebook.CoreKit;
using Firebase.Auth;
using Plugin.Firebase.Auth.Platforms.iOS.Extensions;
using Plugin.Firebase.Auth.Platforms.iOS.Facebook;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using FirebaseAuth = Firebase.Auth.Auth;

namespace Plugin.Firebase.Auth.Facebook;

public sealed class FirebaseAuthFacebookImplementation : DisposableBase, IFirebaseAuthFacebook
{
    public static void Initialize(UIApplication application, NSDictionary launchOptions, string facebookAppId, string facebookDisplayName)
    {
        Settings.AppId = facebookAppId;
        Settings.DisplayName = facebookDisplayName;
        ApplicationDelegate.SharedInstance.FinishedLaunching(application, launchOptions);
    }

    public static void OnActivated(UIApplication application)
    {
        AppEvents.Shared.ActivateApp();
    }

    public static bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
    {
        return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
    }

    private readonly FirebaseAuth _firebaseAuth;
    private readonly Lazy<FacebookAuth> _facebookAuth;

    public FirebaseAuthFacebookImplementation()
    {
        _firebaseAuth = FirebaseAuth.DefaultInstance;
        _facebookAuth = new Lazy<FacebookAuth>(() => new FacebookAuth());
    }

    public async Task<IFirebaseUser> SignInWithFacebookAsync()
    {
        try {
            var credential = await _facebookAuth.Value.GetCredentialAsync(ViewController);
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

    public async Task<IFirebaseUser> LinkWithFacebookAsync()
    {
        try {
            var credential = await _facebookAuth.Value.GetCredentialAsync(ViewController);
            return await LinkWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            _facebookAuth.Value.SignOut();
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
        _facebookAuth.Value.SignOut();
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