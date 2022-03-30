using System.Threading.Tasks;
using Firebase.Auth;
using Foundation;
using Google.SignIn;
using UIKit;

namespace Plugin.Firebase.iOS.Auth.Google
{
    public sealed class GoogleAuth : NSObject, ISignInDelegate
    {
        private UIViewController _viewController;
        private TaskCompletionSource<AuthCredential> _tcs;

        public GoogleAuth()
        {
            SignIn.SharedInstance.Delegate = this;
        }

        public Task<AuthCredential> GetCredentialAsync(UIViewController viewController)
        {
            _viewController = viewController;
            _tcs = new TaskCompletionSource<AuthCredential>();
            SignIn.SharedInstance.PresentingViewController = viewController;
            SignIn.SharedInstance.SignInUser();
            return _tcs.Task;
        }

        public void DidSignIn(SignIn signIn, GoogleUser user, NSError error)
        {
            if(user != null && error == null) {
                _tcs?.SetResult(GoogleAuthProvider.GetCredential(user.Authentication.IdToken, user.Authentication.AccessToken));
            } else {
                _tcs?.SetException(new NSErrorException(error));
            }
        }

        [Export("signInWillDispatch:error:")]
        public void WillDispatch(SignIn signIn, NSError error)
        {
            // needs to be implemented since this class is not a UIViewController
        }

        [Export("signIn:presentViewController:")]
        public void PresentViewController(SignIn signIn, UIViewController viewController)
        {
            _viewController?.PresentViewController(viewController, true, null);
        }

        [Export("signIn:dismissViewController:")]
        public void DismissViewController(SignIn signIn, UIViewController viewController)
        {
            _viewController?.DismissViewController(true, null);
        }

        public void SignOut()
        {
            SignIn.SharedInstance.SignOutUser();
        }
    }
}