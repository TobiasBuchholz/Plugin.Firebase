using System;
using System.Linq;
using Android.Content;
using Android.Gms.Common;
using Firebase.Iid;
using Plugin.Firebase.Abstractions.CloudMessaging;
using Plugin.Firebase.Abstractions.CloudMessaging.EventArgs;
using Plugin.Firebase.Abstractions.Common;

namespace Plugin.Firebase.CloudMessaging
{
    public sealed class FirebaseCloudMessagingImplementation : DisposableBase, IFirebaseCloudMessaging
    {        
        public static void Initialize(Context context)
        {
            _context = context;
        }
        
        private static Context _context;

        public void CheckIfValid()
        {
            var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(_context);
            if (resultCode == ConnectionResult.Success) {
                OnTokenRefresh();
            } else {
                Error?.Invoke(this, new FCMErrorEventArgs(GetErrorMessage(resultCode)));
            }
        }

        private static string GetErrorMessage(int resultCode)
        {
            return GoogleApiAvailability.Instance.IsUserResolvableError(resultCode)
                ? GoogleApiAvailability.Instance.GetErrorString(resultCode)
                : "This device is not supported";
        }

        public void OnTokenRefresh()
        {
            System.Diagnostics.Debug.WriteLine($"token refresh {FirebaseInstanceId.Instance.Token}");
            TokenChanged?.Invoke(this, new FCMTokenChangedEventArgs(FirebaseInstanceId.Instance.Token));
        }

        public void OnNotificationReceived(FCMNotification fcmNotification)
        {
            NotificationReceived?.Invoke(this, new FCMNotificationReceivedEventArgs(fcmNotification));
        }

        public void HandleOpenIntent(Intent intent)
        {
            if(intent.Extras != null) {
                OnNotificationReceived(ConvertToFCMNotification(intent));
            }
        }

        private static FCMNotification ConvertToFCMNotification(Intent intent)
        {
            return new FCMNotification(data: intent
                .Extras
                .KeySet()
                .Select(x => Tuple.Create(x, intent.Extras.GetString(x)))
                .ToList()
                .ToDictionary(x => x.Item1, x => x.Item2));
        } 
        
        public event EventHandler<FCMTokenChangedEventArgs> TokenChanged;
        public event EventHandler<FCMNotificationReceivedEventArgs> NotificationReceived;
        public event EventHandler<FCMErrorEventArgs> Error;
    }
}