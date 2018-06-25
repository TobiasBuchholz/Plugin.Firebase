using System.Threading.Tasks;
using Facebook.LoginKit;
using Firebase.Auth;
using Foundation;
using UIKit;

namespace Plugin.Firebase.iOS.Auth.Facebook
{
    public sealed class FacebookAuth
    {
        public Task<AuthCredential> GetCredentialAsync(UIViewController viewController)
        {
            var tcs = new TaskCompletionSource<AuthCredential>();
            new LoginManager().LogInWithReadPermissions(new []{ "public_profile" }, viewController, (result, error) => {
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
    }
}