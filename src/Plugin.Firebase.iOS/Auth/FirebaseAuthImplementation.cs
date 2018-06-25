using System;
using System.Threading.Tasks;
using Firebase.Auth;
using Foundation;
using Plugin.Firebase.Abstractions.Auth;
using Plugin.Firebase.Auth.PhoneNumber;
using Plugin.Firebase.iOS.Auth.Facebook;
using Plugin.Firebase.iOS.Auth.Google;
using UIKit;
using FirebaseAuth = Firebase.Auth.Auth;

namespace Plugin.Firebase.Auth
{
    public sealed class FirebaseAuthImplementation : BaseFirebaseAuth
    {
        private readonly FirebaseAuth _firebaseAuth;
        private static GoogleAuth _googleAuth;
        private readonly FacebookAuth _facebookAuth;
        private readonly PhoneNumberAuth _phoneNumberAuth;

        public FirebaseAuthImplementation()
        {
            _firebaseAuth = FirebaseAuth.DefaultInstance;
            _googleAuth = new GoogleAuth();
            _facebookAuth = new FacebookAuth();
            _phoneNumberAuth = new PhoneNumberAuth();
        }

        public override async Task<FirebaseUser> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            try {
                var user = await _firebaseAuth.SignInAsync(email, password);
                return CreateFirebaseUser(user);
            } catch(NSErrorException e) {
                if(e.Code == (long) AuthErrorCode.UserNotFound) {
                    var user = await _firebaseAuth.CreateUserAsync(email, password);
                    return CreateFirebaseUser(user);
                }
                throw;
            }
        }

        private static FirebaseUser CreateFirebaseUser(User user)
        {
            return new FirebaseUser(user.Uid, user.DisplayName, user.Email, user.PhotoUrl?.Path, user.IsEmailVerified, user.IsAnonymous);
        }
        
        public override async Task<FirebaseUser> SignInWithGoogleAsync()
        {
            var credential = await _googleAuth.GetCredentialAsync(ViewController);
            return await SignInWithCredentialAsync(credential);
        }

        private async Task<FirebaseUser> SignInWithCredentialAsync(AuthCredential credential)
        {
            var user = await _firebaseAuth.SignInAsync(credential);
            return CreateFirebaseUser(user);
        }

        public override async Task<FirebaseUser> SignInWithFacebookAsync()
        {
            var credential = await _facebookAuth.GetCredentialAsync(ViewController);
            return await SignInWithCredentialAsync(credential);
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
}
