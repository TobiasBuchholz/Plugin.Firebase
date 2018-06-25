using System;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;

namespace Plugin.Firebase.Android.Auth.Google
{
    public sealed class ConnectionCallback : Java.Lang.Object, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        private readonly Action<Bundle> _onConnected;
        private readonly Action<int> _onConnectionSuspended;
        private readonly Action<ConnectionResult> _onConnectionFailed;

        public ConnectionCallback(
            Action<Bundle> onConnected = null,
            Action<int> onConnectionSuspended = null,
            Action<ConnectionResult> onConnectionFailed = null)
        {
            _onConnected = onConnected;
            _onConnectionSuspended = onConnectionSuspended;
            _onConnectionFailed = onConnectionFailed;
        }

        public void OnConnected(Bundle connectionHint)
        {
            _onConnected?.Invoke(connectionHint);
        }

        public void OnConnectionSuspended(int cause)
        {
            _onConnectionSuspended?.Invoke(cause);
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            _onConnectionFailed?.Invoke(result);
        }
    }
}