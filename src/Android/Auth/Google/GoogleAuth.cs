using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Extensions;
using AndroidX.Fragment.App;
using Firebase.Auth;
using GoogleApi = Android.Gms.Auth.Api.Auth;
using GmsTask = Android.Gms.Tasks.Task;

namespace Plugin.Firebase.Android.Auth.Google
{
    public sealed class GoogleAuth
    {
        private const int RequestCodeSignIn = 9001;

        private readonly GoogleSignInClient _signInClient;
        private TaskCompletionSource<AuthCredential> _tcs;
        
        public GoogleAuth(Activity activity, string requestToken)
        {
            _signInClient = GoogleSignIn.GetClient(activity, CreateGoogleSignInOptions(requestToken));
        }

        private static GoogleSignInOptions CreateGoogleSignInOptions(string requestIdToken)
        {
            return new GoogleSignInOptions
                .Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestIdToken(requestIdToken)
                .RequestEmail()
                .Build();
        }
        
        public Task<AuthCredential> GetCredentialAsync(FragmentActivity activity)
        {
            _tcs = new TaskCompletionSource<AuthCredential>();
            activity.StartActivityForResult(_signInClient.SignInIntent, RequestCodeSignIn);
            return _tcs.Task;
        }

        public async Task HandleActivityResultAsync(int requestCode, Result resultCode, Intent data)
        {
            if(requestCode == RequestCodeSignIn) {
                await HandleSignInResultAsync(GoogleSignIn.GetSignedInAccountFromIntent(data));
            }
        }

        private async Task HandleSignInResultAsync(GmsTask signInAccountTask)
        {
            if(signInAccountTask.IsSuccessful) {
                var signInAccount = await signInAccountTask.AsAsync<GoogleSignInAccount>();
                _tcs?.SetResult(GoogleAuthProvider.GetCredential(signInAccount.IdToken, null));
            } else {
                _tcs?.SetException(signInAccountTask.Exception);
            }
        }

        public Task SignOutAsync()
        {
            return _signInClient.SignOutAsync();
        }
    }
}