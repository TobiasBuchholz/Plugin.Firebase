using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Firebase.Auth;
using Plugin.CurrentActivity;
using Plugin.Firebase.Abstractions.Auth;
using Plugin.Firebase.Abstractions.Common;
using Plugin.Firebase.Android.Auth.Email;
using Plugin.Firebase.Android.Auth.Facebook;
using Plugin.Firebase.Android.Auth.Google;
using Plugin.Firebase.Android.Auth.PhoneNumber;
using FirebaseUser = Plugin.Firebase.Abstractions.Auth.FirebaseUser;

namespace Plugin.Firebase.Auth
{
    public sealed class FirebaseAuthImplementation : DisposableBase, IFirebaseAuth
    {
        public static void Initialize(Activity activity, Bundle savedInstanceState, string googleRequestIdToken)
        {
            _googleRequestIdToken = googleRequestIdToken;
            CrossCurrentActivity.Current.Init(activity, savedInstanceState);
        }
        
        public static void HandleActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _googleAuth.HandleActivityResult(requestCode, resultCode, data);
            _facebookAuth.HandleActivityResult(requestCode, resultCode, data);
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
            _googleAuth = new GoogleAuth(FragmentActivity, _googleRequestIdToken);
            _facebookAuth = new FacebookAuth(AppContext);
            _phoneNumberAuth = new PhoneNumberAuth();
        }

        public Task VerifyPhoneNumberAsync(string phoneNumber)
        {
            return _phoneNumberAuth.VerifyPhoneNumberAsync(Activity, phoneNumber);
        }

        public async Task<FirebaseUser> SignInWithCustomTokenAsync(string token)
        {
            var authResult = await _firebaseAuth.SignInWithCustomTokenAsync(token);
            return CreateFirebaseUser(authResult.User, authResult.AdditionalUserInfo);
        }

        private static FirebaseUser CreateFirebaseUser(global::Firebase.Auth.FirebaseUser user, IAdditionalUserInfo additionalUserInfo = null)
        {
            return new FirebaseUser(
                user.Uid,
                user.DisplayName,
                user.Email,
                user.PhotoUrl?.Path, 
                user.IsEmailVerified,
                user.IsAnonymous,
                GetProviderInfos(user.ProviderData, additionalUserInfo));
        }

        private static IEnumerable<ProviderInfo> GetProviderInfos(IEnumerable<IUserInfo> userInfos, IAdditionalUserInfo additionalUserInfo)
        {
            return userInfos.Select(x => new ProviderInfo(
                x.Uid,
                x.ProviderId,
                x.DisplayName,
                x.Email ?? GetEmailFromAdditionalUserInfo(additionalUserInfo),
                x.PhoneNumber,
                x.PhotoUrl?.ToString()));
        }

        private static string GetEmailFromAdditionalUserInfo(IAdditionalUserInfo additionalUserInfo)
        {
            var profile = additionalUserInfo?.Profile;
            if(profile != null && profile.ContainsKey("email")) {
                return profile["email"].ToString();
            }
            return null;
        }
        
        public async Task<FirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode)
        {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await SignInWithCredentialAsync(credential);
        }
        
        private async Task<FirebaseUser> SignInWithCredentialAsync(AuthCredential credential)
        {
            var authResult = await _firebaseAuth.SignInWithCredentialAsync(credential);
            return CreateFirebaseUser(authResult.User, authResult.AdditionalUserInfo);
        }
        
        public async Task<FirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            try {
                var credential = await _emailAuth.GetCredentialAsync(email, password);
                return await SignInWithCredentialAsync(credential);
            } catch(FirebaseAuthInvalidUserException) {
                await _emailAuth.CreateUserAsync(email, password);
                return await SignInWithEmailAndPasswordAsync(email, password);
            }
        }
        
        public async Task<FirebaseUser> SignInWithGoogleAsync()
        {
            var credential = await _googleAuth.GetCredentialAsync(FragmentActivity);
            return await SignInWithCredentialAsync(credential);
        }

        public async Task<FirebaseUser> SignInWithFacebookAsync()
        {
            var credential = await _facebookAuth.GetCredentialAsync(Activity);
            return await SignInWithCredentialAsync(credential);
        }

        public async Task<FirebaseUser> LinkWithPhoneNumberVerificationCodeAsync(string verificationCode)
        {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await LinkWithCredentialAsync(credential);
        }
        
        private async Task<FirebaseUser> LinkWithCredentialAsync(AuthCredential credential)
        {
            var authResult = await _firebaseAuth.CurrentUser.LinkWithCredentialAsync(credential);
            return CreateFirebaseUser(authResult.User, authResult.AdditionalUserInfo);
        }

        public async Task<FirebaseUser> LinkWithEmailAndPasswordAync(string email, string password)
        {
            var credential = await _emailAuth.GetCredentialAsync(email, password);
            return await LinkWithCredentialAsync(credential);
        }

        public async Task<FirebaseUser> LinkWithGoogleAsync()
        {
            try {
                var credential = await _googleAuth.GetCredentialAsync(FragmentActivity);
                return await LinkWithCredentialAsync(credential);
            } catch(Exception) {
                _googleAuth.SignOut();
                throw;
            }
        }

        public async Task<FirebaseUser> LinkWithFacebookAsync()
        {
            try {
                var credential = await _facebookAuth.GetCredentialAsync(Activity);
                return await LinkWithCredentialAsync(credential);
            } catch(Exception) {
                _facebookAuth.SignOut();
                throw;
            }
        }

        public Task SignOutAsync()
        {
            _googleAuth.SignOut();
            _facebookAuth.SignOut();
            _firebaseAuth.SignOut();
            return Task.CompletedTask;
        }

        private static FragmentActivity FragmentActivity =>
            Activity as FragmentActivity ?? throw new NullReferenceException($"Current Activity is either null or not of type {nameof(FragmentActivity)}, which is mandatory for sign in with Google");
        
        private static Activity Activity =>
            CrossCurrentActivity.Current.Activity ?? throw new NullReferenceException("Current Activity is null, ensure that the MainApplication.cs file is setting the CurrentActivity in your source code so the In App Billing can use it.");

        private static Context AppContext =>
            CrossCurrentActivity.Current.AppContext ?? throw new NullReferenceException("AppContext is null, ensure that the MainApplication.cs file is setting the CurrentActivity in your source code so the In App Billing can use it.");

        public FirebaseUser CurrentUser => _firebaseAuth.CurrentUser == null ? null : CreateFirebaseUser(_firebaseAuth.CurrentUser);
    }
}
