using System;
using Android.App;
using Android.OS;
using Firebase;
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
            settings.ThrowWhenConfiguredWrong();
            
            if(firebaseOptions == null) {
                FirebaseApp.InitializeApp(activity);
            } else if(name == null) {
                FirebaseApp.InitializeApp(activity, firebaseOptions);
            } else {
                FirebaseApp.InitializeApp(activity, firebaseOptions, name);
            }

            if(settings?.IsAuthEnabled ?? false) {
                FirebaseAuthImplementation.Initialize(activity, savedInstanceState, settings.GoogleRequestIdToken);
            }
            
            Console.WriteLine($"Plugin.Firebase initialized with the following settings:\n{settings}");
        }
    }
}