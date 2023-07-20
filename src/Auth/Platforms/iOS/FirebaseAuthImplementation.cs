using Firebase.Auth;
using Google.SignIn;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices;
using Plugin.Firebase.Auth.Platforms.iOS.Email;
using Plugin.Firebase.Auth.Platforms.iOS.Extensions;
using Plugin.Firebase.Auth.Platforms.iOS.Google;
using Plugin.Firebase.Auth.Platforms.iOS.PhoneNumber;
using Plugin.Firebase.Core;
using Plugin.Firebase.Core.Exceptions;
using FirebaseAuth = Firebase.Auth.Auth;
using Task = System.Threading.Tasks.Task;
using CrossActionCodeSettings = Plugin.Firebase.Auth.ActionCodeSettings;

namespace Plugin.Firebase.Auth;

public sealed class FirebaseAuthImplementation : DisposableBase, IFirebaseAuth
{
    public static void Initialize()
    {
        var googleServiceDictionary = NSDictionary.FromFile("GoogleService-Info.plist");
        SignIn.SharedInstance.ClientId = googleServiceDictionary["CLIENT_ID"].ToString();
    }

    public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
    {
        return SignIn.SharedInstance.HandleUrl(url);
    }

    private readonly FirebaseAuth _firebaseAuth;
    private readonly EmailAuth _emailAuth;
    private static Lazy<GoogleAuth> _googleAuth;
    private readonly PhoneNumberAuth _phoneNumberAuth;

    public FirebaseAuthImplementation()
    {
        _firebaseAuth = FirebaseAuth.DefaultInstance;
        _emailAuth = new EmailAuth();
        _googleAuth = new Lazy<GoogleAuth>(() => new GoogleAuth());
        _phoneNumberAuth = new PhoneNumberAuth();

        // apply the default app language for sending emails
        _firebaseAuth.UseAppLanguage();
    }

    public async Task VerifyPhoneNumberAsync(string phoneNumber)
    {
        try {
            await _phoneNumberAuth.VerifyPhoneNumberAsync(ViewController, phoneNumber);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
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

    public async Task<IFirebaseUser> SignInWithCustomTokenAsync(string token)
    {
        try {
            var user = await _firebaseAuth.SignInWithCustomTokenAsync(token);
            return user.User.ToAbstract();
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private static IEnumerable<ProviderInfo> GetProviderInfos(IEnumerable<IUserInfo> userInfos)
    {
        return userInfos.Select(x => new ProviderInfo(x.Uid, x.ProviderId, x.DisplayName, x.Email, x.PhoneNumber, x.PhotoUrl?.AbsoluteString));
    }

    public async Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode)
    {
        try {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
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

    public async Task<IFirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password, bool createsUserAutomatically = true)
    {
        try {
            var credential = await _emailAuth.GetCredentialAsync(email, password);
            return await SignInWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            if(e.Code == (long) AuthErrorCode.UserNotFound && createsUserAutomatically) {
                return await CreateUserAsync(email, password);
            }
            throw GetFirebaseAuthException(e);
        }
    }

    public async Task<IFirebaseUser> CreateUserAsync(string email, string password)
    {
        try {
            return await _emailAuth.CreateUserAsync(email, password);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public async Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link)
    {
        try {
            var authResult = await _firebaseAuth.SignInWithLinkAsync(email, link);
            return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
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

    public async Task<IFirebaseUser> SignInWithAppleAsync()
    {
        try {
            WebAuthenticatorResult appleAuthResult = null;
            OAuthCredential credential = null;

            if(DeviceInfo.Platform == DevicePlatform.iOS && DeviceInfo.Version.Major >= 13) {
                var options = new AppleSignInAuthenticator.Options { IncludeEmailScope = true };
                appleAuthResult = await AppleSignInAuthenticator.AuthenticateAsync(options);
            } else {
                throw new PlatformNotSupportedException();
            }

            if(appleAuthResult != null && !String.IsNullOrEmpty(appleAuthResult.IdToken)) {
                credential = OAuthProvider.GetCredential("apple.com", appleAuthResult.IdToken, null);
            } else {
                throw new ApplicationException("Cannot authenticate user with Native Apple Sign In API");
            }

            return await SignInWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            throw new FirebaseException(e.Error?.LocalizedDescription);
        }
    }

    public async Task<IFirebaseUser> SignInAnonymouslyAsync()
    {
        try {
            var authResult = await _firebaseAuth.SignInAnonymouslyAsync();
            return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public async Task<IFirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode)
    {
        try {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await LinkWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    private async Task<IFirebaseUser> LinkWithCredentialAsync(AuthCredential credential)
    {
        var authResult = await _firebaseAuth.CurrentUser.LinkAsync(credential);
        return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
    }

    public async Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password)
    {
        try {
            var credential = await _emailAuth.GetCredentialAsync(email, password);
            return await LinkWithCredentialAsync(credential);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
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

    public async Task<string[]> FetchSignInMethodsAsync(string email)
    {
        try {
            return await _firebaseAuth.FetchSignInMethodsAsync(email);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public async Task SendSignInLink(string toEmail, CrossActionCodeSettings actionCodeSettings)
    {
        try {
            await _firebaseAuth.SendSignInLinkAsync(toEmail, actionCodeSettings.ToNative());
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public Task SignOutAsync()
    {
        _googleAuth.Value.SignOut();
        _firebaseAuth.SignOut(out var e);
        return Task.CompletedTask;
    }

    public bool IsSignInWithEmailLink(string link)
    {
        try {
            return _firebaseAuth.IsSignIn(link);
        } catch(NSErrorException e) {
            throw GetFirebaseAuthException(e);
        }
    }

    public Task SendPasswordResetEmailAsync()
    {
        if(_firebaseAuth.CurrentUser == null) {
            throw new FirebaseException("CurrentUser is null. You need to be logged in to use this feature.");
        } else {
            return _firebaseAuth.SendPasswordResetAsync(_firebaseAuth.CurrentUser.Email);
        }
    }

    public Task SendPasswordResetEmailAsync(string email)
    {
        return _firebaseAuth.SendPasswordResetAsync(email);
    }

    public void UseEmulator(string host, int port)
    {
        _firebaseAuth.UseEmulatorWithHost(host, port);
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

    public IFirebaseUser CurrentUser => _firebaseAuth.CurrentUser?.ToAbstract();
}
