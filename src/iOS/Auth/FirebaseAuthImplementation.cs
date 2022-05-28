using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facebook.CoreKit;
using Firebase.Auth;
using Foundation;
using Google.SignIn;
using Plugin.Firebase.Auth.PhoneNumber;
using Plugin.Firebase.Common;
using Plugin.Firebase.iOS.Auth;
using Plugin.Firebase.iOS.Auth.Email;
using Plugin.Firebase.iOS.Auth.Facebook;
using Plugin.Firebase.iOS.Auth.Google;
using UIKit;
using FirebaseAuth = Firebase.Auth.Auth;
using Task = System.Threading.Tasks.Task;
using CrossActionCodeSettings = Plugin.Firebase.Auth.ActionCodeSettings;

namespace Plugin.Firebase.Auth
{
    public sealed class FirebaseAuthImplementation : DisposableBase, IFirebaseAuth
    {
        public static void Initialize(UIApplication application, NSDictionary launchOptions, string facebookAppId, string facebookDisplayName)
        {
            var googleServiceDictionary = NSDictionary.FromFile("GoogleService-Info.plist");
            SignIn.SharedInstance.ClientId = googleServiceDictionary["CLIENT_ID"].ToString();

            Settings.AppId = facebookAppId;
            Settings.DisplayName = facebookDisplayName;
            ApplicationDelegate.SharedInstance.FinishedLaunching(application, launchOptions);
        }

        public static void OnActivated(UIApplication application)
        {
            AppEvents.Shared.ActivateApp();
        }

        public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            return SignIn.SharedInstance.HandleUrl(url);
        }

        public static bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }

        private readonly FirebaseAuth _firebaseAuth;
        private readonly EmailAuth _emailAuth;
        private static GoogleAuth _googleAuth;
        private readonly FacebookAuth _facebookAuth;
        private readonly PhoneNumberAuth _phoneNumberAuth;

        public FirebaseAuthImplementation()
        {
            _firebaseAuth = FirebaseAuth.DefaultInstance;
            _emailAuth = new EmailAuth();
            _googleAuth = new GoogleAuth();
            _facebookAuth = new FacebookAuth();
            _phoneNumberAuth = new PhoneNumberAuth();
        }

        public async Task VerifyPhoneNumberAsync(string phoneNumber)
        {
            try {
                await _phoneNumberAuth.VerifyPhoneNumberAsync(ViewController, phoneNumber);
            } catch(NSErrorException e) {
                throw GetFirebaseAuthException(e);
            }
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
                    await CreateUserAsync(email, password);
                    return await SignInWithEmailAndPasswordAsync(email, password, false);
                } else {
                    throw GetFirebaseAuthException(e);
                }
            }
        }

        public Task CreateUserAsync(string email, string password)
        {
            try {
                return _emailAuth.CreateUserAsync(email, password);
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
                var credential = await _googleAuth.GetCredentialAsync(ViewController);
                return await SignInWithCredentialAsync(credential);
            } catch(NSErrorException e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task<IFirebaseUser> SignInWithFacebookAsync()
        {
            try {
                var credential = await _facebookAuth.GetCredentialAsync(ViewController);
                return await SignInWithCredentialAsync(credential);
            } catch(NSErrorException e) {
                throw GetFirebaseAuthException(e);
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
                var credential = await _googleAuth.GetCredentialAsync(ViewController);
                return await LinkWithCredentialAsync(credential);
            } catch(NSErrorException e) {
                _googleAuth.SignOut();
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task<IFirebaseUser> LinkWithFacebookAsync()
        {
            try {
                var credential = await _facebookAuth.GetCredentialAsync(ViewController);
                return await LinkWithCredentialAsync(credential);
            } catch(NSErrorException e) {
                _facebookAuth.SignOut();
                throw GetFirebaseAuthException(e);
            }
        }

        public Task<string[]> FetchSignInMethodsAsync(string email)
        {
            try {
                return _firebaseAuth.FetchSignInMethodsAsync(email);
            } catch(NSErrorException e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public Task SendSignInLink(string toEmail, CrossActionCodeSettings actionCodeSettings)
        {
            try {
                return _firebaseAuth.SendSignInLinkAsync(toEmail, actionCodeSettings.ToNative());
            } catch(NSErrorException e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public Task SignOutAsync()
        {
            _googleAuth.SignOut();
            _facebookAuth.SignOut();
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

        private static FirebaseAuthException GetFirebaseAuthException(NSErrorException ex)
        {
            AuthErrorCode errorCode;
            if(IntPtr.Size == 8) // 64 bits devices
                errorCode = (AuthErrorCode) ((long) ex.Error.Code);
            else // 32 bits devices
                errorCode = (AuthErrorCode) ((int) ex.Error.Code);

            FIRAuthError authError;
            Enum.TryParse(errorCode.ToString(), out authError);

            return new FirebaseAuthException(authError, ex.Error.LocalizedDescription);
        }
    }
}