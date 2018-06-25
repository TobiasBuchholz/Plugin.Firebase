using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Firebase.Auth;
using Plugin.CurrentActivity;
using Plugin.Firebase.Abstractions.Auth;
using Plugin.Firebase.Android.Auth.Facebook;
using Plugin.Firebase.Android.Auth.Google;
using Plugin.Firebase.Android.Auth.PhoneNumber;
using FirebaseUser = Plugin.Firebase.Abstractions.Auth.FirebaseUser;

namespace Plugin.Firebase.Auth
{
    public sealed class FirebaseAuthImplementation : BaseFirebaseAuth
    {
        private readonly FirebaseAuth _firebaseAuth;
        private static GoogleAuth _googleAuth;
        private static FacebookAuth _facebookAuth;
        private readonly PhoneNumberAuth _phoneNumberAuth;
        
        public FirebaseAuthImplementation()
        {
            _firebaseAuth = FirebaseAuth.Instance;
            _googleAuth = new GoogleAuth(FragmentActivity, RequestIdToken);
            _facebookAuth = new FacebookAuth(AppContext);
            _phoneNumberAuth = new PhoneNumberAuth();
        }

        public override async Task<FirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            try {
                var result = await _firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);
                return CreateFirebaseUser(result.User);
            } catch(FirebaseAuthInvalidUserException) {
                var result = await _firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password);
                return CreateFirebaseUser(result.User);
            }
        }

        private static FirebaseUser CreateFirebaseUser(global::Firebase.Auth.FirebaseUser user)
        {
            return new FirebaseUser(user.Uid, user.DisplayName, user.Email, user.PhotoUrl?.Path, user.IsEmailVerified, user.IsAnonymous);
        }
        
        public override async Task<FirebaseUser> SignInWithGoogleAsync()
        {
            var token = await _googleAuth.SignInAsync(FragmentActivity);
            var credential = GoogleAuthProvider.GetCredential(token, null);
            return await SignInWithCredentialAsync(credential);
        }
        
        private async Task<FirebaseUser> SignInWithCredentialAsync(AuthCredential credential)
        {
            var authResult = await _firebaseAuth.SignInWithCredentialAsync(credential);
            var user = authResult.User;
            return CreateFirebaseUser(user);
        }

        public override async Task<FirebaseUser> SignInWithFacebookAsync()
        {
            var token = await _facebookAuth.SignInAsync(Activity);
            var credential = FacebookAuthProvider.GetCredential(token);
            return await SignInWithCredentialAsync(credential);
        }

        public override Task VerifyPhoneNumberAsync(string phoneNumber)
        {
            return _phoneNumberAuth.VerifyPhoneNumberAsync(Activity, phoneNumber);
        }

        public override async Task<FirebaseUser> SignInWithPhoneNumberVerificationCodeAsync(string verificationCode)
        {
            var credential = await _phoneNumberAuth.GetCredentialAsync(verificationCode);
            return await SignInWithCredentialAsync(credential);
        }

        public static void HandleActivityResult(int requestCode, Result resultCode, Intent data)
        {
            _googleAuth.HandleActivityResult(requestCode, resultCode, data);
            _facebookAuth.HandleActivityResult(requestCode, resultCode, data);
        }
        
        public static string RequestIdToken { private get; set; }

        private static FragmentActivity FragmentActivity =>
            Activity as FragmentActivity ?? throw new NullReferenceException($"Current Activity is either null or not of type {nameof(FragmentActivity)}, which is mandatory for sign in with Google");
        
        private static Activity Activity =>
            CrossCurrentActivity.Current.Activity ?? throw new NullReferenceException("Current Activity is null, ensure that the MainApplication.cs file is setting the CurrentActivity in your source code so the In App Billing can use it.");

        private static Context AppContext =>
            CrossCurrentActivity.Current.AppContext ?? throw new NullReferenceException("AppContext is null, ensure that the MainApplication.cs file is setting the CurrentActivity in your source code so the In App Billing can use it.");
    }
}
