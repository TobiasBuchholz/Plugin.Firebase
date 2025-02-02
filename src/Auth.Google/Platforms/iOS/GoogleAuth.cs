using Firebase.Auth;
using Google.SignIn;

namespace Plugin.Firebase.Auth.Platforms.iOS.Google;

public sealed class GoogleAuth : NSObject
{
    private UIViewController _viewController;
    private TaskCompletionSource<AuthCredential> _tcs;

    public Task<AuthCredential> GetCredentialAsync(UIViewController viewController)
    {
        _viewController = viewController;
        _tcs = new TaskCompletionSource<AuthCredential>();
        SignIn.SharedInstance.SignInWithPresentingViewController(viewController, DidSignIn);
        return _tcs.Task;
    }

    public void DidSignIn(SignInResult signIn, NSError error)
    {
        var user = signIn.User;

        if(user != null && error == null) {
            _tcs?.SetResult(GoogleAuthProvider.GetCredential(user.IdToken.TokenString, user.AccessToken.TokenString));
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