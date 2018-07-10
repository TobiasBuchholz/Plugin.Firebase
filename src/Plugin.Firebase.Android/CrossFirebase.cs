using Android.Content;
using Firebase;

namespace Plugin.Firebase.Android
{
    public static class CrossFirebase
    {
        public static void Initialize(Context context)
        {
            FirebaseApp.InitializeApp(context);
        } 
    }
}