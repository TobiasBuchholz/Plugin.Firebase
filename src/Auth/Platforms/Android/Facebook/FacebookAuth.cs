using Android.App;
using Android.Content;
using Firebase.Auth;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;

namespace Plugin.Firebase.Android.Auth.Facebook;

public sealed class FacebookAuth
{
    private readonly ICallbackManager _callbackManager;

    public FacebookAuth()
    {
        FacebookSdk.FullyInitialize();
        _callbackManager = CallbackManagerFactory.Create();
    }

    public Task<AuthCredential> GetCredentialAsync(Activity activity)
    {
        var tcs = new TaskCompletionSource<AuthCredential>();
        var callback = new FacebookCallback<LoginResult>(
            onSuccess: x => tcs.SetResult(FacebookAuthProvider.GetCredential(x.AccessToken.Token)),
            onCancel: tcs.SetCanceled,
            onError: tcs.SetException);

        LoginManager.Instance.RegisterCallback(_callbackManager, callback);
        LoginManager.Instance.LogInWithReadPermissions(activity, new List<string> { "public_profile", "email" });
        return tcs.Task;
    }

    public void HandleActivityResult(int requestCode, Result resultCode, Intent data)
    {
        _callbackManager.OnActivityResult(requestCode, (int) resultCode, data);
    }

    public void SignOut()
    {
        LoginManager.Instance.LogOut();
    }
}