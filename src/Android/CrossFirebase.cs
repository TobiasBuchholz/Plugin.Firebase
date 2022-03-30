using System;
using Android.App;
using Android.OS;
using Firebase;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Shared;

namespace Plugin.Firebase.Android
{
    public static class CrossFirebase
    {
        public static void Initialize(
            Activity activity,
            Bundle savedInstanceState,
            CrossFirebaseSettings settings,
            FirebaseOptions firebaseOptions = null,
            string name = null)
        {
            if(firebaseOptions == null) {
                FirebaseApp.InitializeApp(activity);
            } else if(name == null) {
                FirebaseApp.InitializeApp(activity, firebaseOptions);
            } else {
                FirebaseApp.InitializeApp(activity, firebaseOptions, name);
            }

            if(settings.IsAnalyticsEnabled) {
                FirebaseAnalyticsImplementation.Initialize(activity);
            }

            if(settings.IsAuthEnabled) {
                FirebaseAuthImplementation.Initialize(activity, savedInstanceState, settings.GoogleRequestIdToken ?? "123-abc");
            }

            Console.WriteLine($"Plugin.Firebase initialized with the following settings:\n{settings}");
        }
    }
}