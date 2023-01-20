using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Extensions;
using Android.OS;
using AndroidX.Fragment.App;
using Firebase.Auth;
using Microsoft.Maui.ApplicationModel;
using Plugin.Firebase.Android.Auth;
using Plugin.Firebase.Android.Auth.Email;
using Plugin.Firebase.Android.Auth.Facebook;
using Plugin.Firebase.Android.Auth.Google;
using Plugin.Firebase.Android.Auth.PhoneNumber;
using Plugin.Firebase.Common;
using CrossActionCodeSettings = Plugin.Firebase.Auth.ActionCodeSettings;
using CrossFirebaseAuthException = Plugin.Firebase.Common.FirebaseAuthException;

namespace Plugin.Firebase.Auth
{
    public sealed class FirebaseAuthImplementation : DisposableBase, IFirebaseAuth
    {
        public static void Initialize(Activity activity, Bundle savedInstanceState, string googleRequestIdToken)
        {
            _googleRequestIdToken = googleRequestIdToken;
        }

        public static Task HandleActivityResultAsync(int requestCode, Result resultCode, Intent data)
        {
            _facebookAuth.HandleActivityResult(requestCode, resultCode, data);
            return _googleAuth.HandleActivityResultAsync(requestCode, resultCode, data);
        }

        private readonly FirebaseAuth _firebaseAuth;
        private readonly EmailAuth _emailAuth;
        private static GoogleAuth _googleAuth;
        private static FacebookAuth _facebookAuth;
        private readonly PhoneNumberAuth _phoneNumberAuth;
        private static string _googleRequestIdToken;

        public FirebaseAuthImplementation()
        {
            _firebaseAuth = FirebaseAuth.Instance;
            _emailAuth = new EmailAuth();
            _googleAuth = new GoogleAuth(Activity, _googleRequestIdToken);
            _facebookAuth = new FacebookAuth();
            _phoneNumberAuth = new PhoneNumberAuth();

            // apply the default app language for sending emails 
            _firebaseAuth.UseAppLanguage();
        }

        public async Task VerifyPhoneNumberAsync(string phoneNumber)
        {
            try {
                await _phoneNumberAuth.VerifyPhoneNumberAsync(Activity, phoneNumber);
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        private static CrossFirebaseAuthException GetFirebaseAuthException(Exception ex)
        {
            return ex switch {
                FirebaseAuthEmailException => new CrossFirebaseAuthException(FIRAuthError.InvalidEmail, ex.Message),
                FirebaseAuthInvalidUserException => new CrossFirebaseAuthException(FIRAuthError.UserNotFound, ex.Message),
                FirebaseAuthWeakPasswordException => new CrossFirebaseAuthException(FIRAuthError.WeakPassword, ex.Message),
                FirebaseAuthInvalidCredentialsException { ErrorCode: "ERROR_WRONG_PASSWORD" } => new CrossFirebaseAuthException(FIRAuthError.WrongPassword, ex.Message),
                FirebaseAuthInvalidCredentialsException => new CrossFirebaseAuthException(FIRAuthError.InvalidCredential, ex.Message),
                FirebaseAuthUserCollisionException { ErrorCode: "ERROR_EMAIL_ALREADY_IN_USE" } => new CrossFirebaseAuthException(FIRAuthError.EmailAlreadyInUse, ex.Message),
                FirebaseAuthUserCollisionException { ErrorCode: "ERROR_ACCOUNT_EXISTS_WITH_DIFFERENT_CREDENTIAL" } => new CrossFirebaseAuthException(FIRAuthError.AccountExistsWithDifferentCredential, ex.Message),
                _ => new CrossFirebaseAuthException(FIRAuthError.Undefined, ex.Message)
            };
        }

        public async Task<IFirebaseUser> SignInWithCustomTokenAsync(string token)
        {
            try {
                var authResult = await _firebaseAuth.SignInWithCustomTokenAsync(token);
                return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task<IFirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode)
        {
            try {
                var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
                return await SignInWithCredentialAsync(credential);
            } catch(Exception e) {
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
            } catch(Exception e) {
                if(e is FirebaseAuthInvalidUserException && createsUserAutomatically) {
                    await CreateUserAsync(email, password);
                    return await SignInWithEmailAndPasswordAsync(email, password, false);
                }
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task CreateUserAsync(string email, string password)
        {
            try {
                await _emailAuth.CreateUserAsync(email, password);
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task<IFirebaseUser> SignInWithEmailLinkAsync(string email, string link)
        {
            try {
                await _firebaseAuth.SignInWithEmailLink(email, link);
                return _firebaseAuth.CurrentUser.ToAbstract();
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task<IFirebaseUser> SignInWithGoogleAsync()
        {
            try {
                var credential = await _googleAuth.GetCredentialAsync(FragmentActivity);
                return await SignInWithCredentialAsync(credential);
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task<IFirebaseUser> SignInWithFacebookAsync()
        {
            try {
                var credential = await _facebookAuth.GetCredentialAsync(Activity);
                return await SignInWithCredentialAsync(credential);
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public Task<IFirebaseUser> SignInWithAppleAsync()
        {
            throw new PlatformNotSupportedException();
        }

        public async Task<IFirebaseUser> SignInAnonymouslyAsync()
        {
            try {
                var authResult = await _firebaseAuth.SignInAnonymouslyAsync();
                return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task<IFirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode)
        {
            try {
                var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
                return await LinkWithCredentialAsync(credential);
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        private async Task<IFirebaseUser> LinkWithCredentialAsync(AuthCredential credential)
        {
            var authResult = await _firebaseAuth.CurrentUser.LinkWithCredentialAsync(credential);
            return authResult.User.ToAbstract(authResult.AdditionalUserInfo);
        }

        public async Task<IFirebaseUser> LinkWithEmailAndPasswordAsync(string email, string password)
        {
            try {
                var credential = await _emailAuth.GetCredentialAsync(email, password);
                return await LinkWithCredentialAsync(credential);
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task<IFirebaseUser> LinkWithGoogleAsync()
        {
            try {
                var credential = await _googleAuth.GetCredentialAsync(FragmentActivity);
                return await LinkWithCredentialAsync(credential);
            } catch(Exception e) {
                await _googleAuth.SignOutAsync();
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task<IFirebaseUser> LinkWithFacebookAsync()
        {
            try {
                var credential = await _facebookAuth.GetCredentialAsync(Activity);
                return await LinkWithCredentialAsync(credential);
            } catch(Exception e) {
                _facebookAuth.SignOut();
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task<string[]> FetchSignInMethodsAsync(string email)
        {
            try {
                var result = await _firebaseAuth.FetchSignInMethodsForEmail(email).AsAsync<ISignInMethodQueryResult>();
                return result?.SignInMethods?.ToArray();
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task SendSignInLink(string toEmail, CrossActionCodeSettings actionCodeSettings)
        {
            try {
                await _firebaseAuth.SendSignInLinkToEmail(toEmail, actionCodeSettings.ToNative());
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public async Task SignOutAsync()
        {
            try {
                _firebaseAuth.SignOut();
                await _googleAuth.SignOutAsync();
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public bool IsSignInWithEmailLink(string link)
        {
            try {
                return _firebaseAuth.IsSignInWithEmailLink(link);
            } catch(Exception e) {
                throw GetFirebaseAuthException(e);
            }
        }

        public Task SendPasswordResetEmailAsync()
        {
            if(_firebaseAuth.CurrentUser == null) {
                throw new FirebaseException("CurrentUser is null. You need to be logged in to use this feature.");
            } else {
                return _firebaseAuth.SendPasswordResetEmailAsync(_firebaseAuth.CurrentUser.Email);
            }
        }

        public Task SendPasswordResetEmailAsync(string email)
        {
            return _firebaseAuth.SendPasswordResetEmailAsync(email);
        }

        public void UseEmulator(string host, int port)
        {
            _firebaseAuth.UseEmulator(host, port);
        }

        private static FragmentActivity FragmentActivity =>
            Activity as FragmentActivity ?? throw new NullReferenceException($"Current Activity is either null or not of type {nameof(FragmentActivity)}, which is mandatory for sign in with Google");

        private static Activity Activity =>
            Platform.CurrentActivity ?? throw new NullReferenceException("Current Activity is null, ensure that the MainApplication.cs file is setting the CurrentActivity in your source code so Firebase Analytics can use it.");

        private static Context AppContext =>
            Platform.AppContext ?? throw new NullReferenceException("AppContext is null, ensure that the MainApplication.cs file is setting the CurrentActivity in your source code so the Firebase Analytics can use it.");

        public IFirebaseUser CurrentUser => _firebaseAuth.CurrentUser?.ToAbstract();
    }
}