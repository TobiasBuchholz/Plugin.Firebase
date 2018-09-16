using Android.App;
using Firebase.Iid;
using Plugin.Firebase.CloudMessaging;

namespace Plugin.Firebase.Android.IntentServices
{
    [Service]
    [IntentFilter(new [] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseInstanceIdService : FirebaseInstanceIdService
    {
        public override void OnTokenRefresh()
        {
            base.OnTokenRefresh();
            CrossFirebaseCloudMessaging.Current.OnTokenRefresh();
        }
    }
}
