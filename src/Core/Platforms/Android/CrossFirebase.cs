using System;
using Android.App;
using Firebase;

namespace Plugin.Firebase.Android;

public static class CrossFirebase
{
    public static void Initialize(
        Activity activity,
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
    }
}