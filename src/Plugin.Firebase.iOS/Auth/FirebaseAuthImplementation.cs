using System;
using System.Threading.Tasks;
using Facebook.CoreKit;
using Firebase.Auth;
using Foundation;
using Google.SignIn;
using Plugin.Firebase.Abstractions.Auth;
using Plugin.Firebase.Auth.PhoneNumber;
using Plugin.Firebase.iOS.Auth.Email;
using Plugin.Firebase.iOS.Auth.Facebook;
using Plugin.Firebase.iOS.Auth.Google;
using UIKit;
using FirebaseAuth = Firebase.Auth.Auth;
using Task = System.Threading.Tasks.Task;

namespace Plugin.Firebase.Auth
{
    public sealed class FirebaseAuthImplementation : BaseFirebaseAuth
    {
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

        public override async Task VerifyPhoneNumberAsync(string phoneNumber)
        {
            await _phoneNumberAuth.VerifyPhoneNumberAsync(ViewController, phoneNumber);
        }

        public override async Task<FirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode)
        {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await SignInWithCredentialAsync(credential);
        }
        
        private async Task<FirebaseUser> SignInWithCredentialAsync(AuthCredential credential)
        {
            var user = await _firebaseAuth.SignInAsync(credential);
            return CreateFirebaseUser(user);
        }
        
        private static FirebaseUser CreateFirebaseUser(User user)
        {
            return new FirebaseUser(user.Uid, user.DisplayName, user.Email, user.PhotoUrl?.AbsoluteString, user.IsEmailVerified, user.IsAnonymous);
        }
        
        public override async Task<FirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            try {
                var credential = await _emailAuth.GetCredentialAsync(email, password);
                return await SignInWithCredentialAsync(credential);
            } catch(NSErrorException e) {
                if(e.Code == (long) AuthErrorCode.UserNotFound) {
                    await _emailAuth.CreateUserAsync(email, password);
                    return await SignInWithEmailAndPasswordAsync(email, password);
                }
                throw;
            }
        }
        
        public override async Task<FirebaseUser> SignInWithGoogleAsync()
        {
            var credential = await _googleAuth.GetCredentialAsync(ViewController);
            return await SignInWithCredentialAsync(credential);
        }


        public override async Task<FirebaseUser> SignInWithFacebookAsync()
        {
            var credential = await _facebookAuth.GetCredentialAsync(ViewController);
            return await SignInWithCredentialAsync(credential);
        }

        public override async Task<FirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode)
        {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await LinkWithCredentialAsync(credential);
        }
        
        private async Task<FirebaseUser> LinkWithCredentialAsync(AuthCredential credential)
        {
            var user = await _firebaseAuth.CurrentUser.LinkAsync(credential);
            var data = user.ProviderData[0];
            return new FirebaseUser(user.Uid, data.DisplayName, user.Email, data.PhotoUrl?.AbsoluteString, user.IsEmailVerified, user.IsAnonymous);
        }
        
        public override async Task<FirebaseUser> LinkWithEmailAndPasswordAync(string email, string password)
        {
            var credential = await _emailAuth.GetCredentialAsync(email, password);
            return await LinkWithCredentialAsync(credential);
        }
        
        public override async Task<FirebaseUser> LinkWithGoogleAsync()
        {
            var credential = await _googleAuth.GetCredentialAsync(ViewController);
            return await LinkWithCredentialAsync(credential);
        }

        public override async Task<FirebaseUser> LinkWithFacebookAsync()
        {
            var credential = await _facebookAuth.GetCredentialAsync(ViewController);
            return await LinkWithCredentialAsync(credential);
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
        
        public static void Initialize(UIApplication application, NSDictionary launchOptions, string facebookAppId, string facebookDisplayName)
        {
            var googleServiceDictionary = NSDictionary.FromFile("GoogleService-Info.plist");
            SignIn.SharedInstance.ClientID = googleServiceDictionary["CLIENT_ID"].ToString();
            
            Settings.AppID = facebookAppId;
            Settings.DisplayName = facebookDisplayName;
            ApplicationDelegate.SharedInstance.FinishedLaunching(application, launchOptions);
        }

        public static void OnActivated(UIApplication application)
        {
            AppEvents.ActivateApp();
        }
        
        public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var openUrlOptions = new UIApplicationOpenUrlOptions(options);
            return SignIn.SharedInstance.HandleUrl(url, openUrlOptions.SourceApplication, openUrlOptions.Annotation);
        }
        
        public static bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            return ApplicationDelegate.SharedInstance.OpenUrl(application, url, sourceApplication, annotation);
        }
    }
}
