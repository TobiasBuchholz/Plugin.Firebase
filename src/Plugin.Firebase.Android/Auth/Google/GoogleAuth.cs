using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common.Apis;
using Android.Support.V4.App;
using Firebase.Auth;
using GoogleApi = Android.Gms.Auth.Api.Auth;

namespace Plugin.Firebase.Android.Auth.Google
{
    public sealed class GoogleAuth
    {
        private const int RequestCodeSignIn = 9001;

        private readonly GoogleApiClient _googleApiClient;
        private TaskCompletionSource<AuthCredential> _tcs;
        
        public GoogleAuth(FragmentActivity activity, string requestToken)
        {
            _googleApiClient = CreateGoogleApiClient(activity, requestToken);
        }

        private GoogleApiClient CreateGoogleApiClient(FragmentActivity activity, string requestIdToken)
        {
            var callback = CreateConnectionCallback();
            return new GoogleApiClient
                .Builder(activity)
                .EnableAutoManage(activity, callback)
                .AddApi(GoogleApi.GOOGLE_SIGN_IN_API, CreateGoogleSignInOptions(requestIdToken))
                .AddConnectionCallbacks(callback)
                .Build();
        }

        private ConnectionCallback CreateConnectionCallback()
        {
            return new ConnectionCallback(onConnectionFailed: x => _tcs.SetException(new Exception(x.ErrorMessage)));
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
            var signInIntent = GoogleApi.GoogleSignInApi.GetSignInIntent(_googleApiClient);
            activity.StartActivityForResult(signInIntent, RequestCodeSignIn);
            return _tcs.Task;
        }

        public void HandleActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if(requestCode == RequestCodeSignIn) {
                HandleSignInResult(GoogleApi.GoogleSignInApi.GetSignInResultFromIntent(data));
            }
        }

        private void HandleSignInResult(GoogleSignInResult loginResult)
        {
            if(loginResult.IsSuccess) {
                _tcs?.SetResult(GoogleAuthProvider.GetCredential(loginResult.SignInAccount.IdToken, null));
            } else {
                _tcs?.SetException(new Exception($"Google sign in failed: status = {loginResult.Status}"));
            }
        }

        public void SignOut()
        {
            GoogleApi.GoogleSignInApi.SignOut(_googleApiClient);
        }
    }
}