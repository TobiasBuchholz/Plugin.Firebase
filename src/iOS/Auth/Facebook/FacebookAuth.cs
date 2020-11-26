using System.Threading.Tasks;
using Facebook.LoginKit;
using Firebase.Auth;
using Foundation;
using UIKit;

namespace Plugin.Firebase.iOS.Auth.Facebook
{
    public sealed class FacebookAuth
    {
        private readonly LoginManager _loginManager;
        
        public FacebookAuth()
        {
            _loginManager = new LoginManager();
        }

        public Task<AuthCredential> GetCredentialAsync(UIViewController viewController)
        {
            var tcs = new TaskCompletionSource<AuthCredential>();
            _loginManager.LogIn(new []{ "public_profile", "email" }, viewController, (result, error) => {
                if(result != null && error == null) {
                    if(result.IsCancelled) {
                        tcs.SetCanceled();
                    } else {
                        tcs.SetResult(FacebookAuthProvider.GetCredential(result.Token.TokenString));
                    }
                } else {
                    tcs.SetException(new NSErrorException(error));
                }
            });
            return tcs.Task;
        }

        public void SignOut()
        {
            _loginManager.LogOut();
        }
    }
}