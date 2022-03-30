using System;
using Firebase.Core;
using Foundation;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Shared;
using UIKit;

namespace Plugin.Firebase.iOS
{
    public static class CrossFirebase
    {
        public static void Initialize(
            UIApplication app,
            NSDictionary options,
            CrossFirebaseSettings settings,
            Options firebaseOptions = null,
            string name = null)
        {
            if(firebaseOptions == null) {
                App.Configure();
            } else if(name == null) {
                App.Configure(firebaseOptions);
            } else {
                App.Configure(name, firebaseOptions);
            }

            if(settings.IsAnalyticsEnabled) {
                FirebaseAnalyticsImplementation.Initialize();
            }

            if(settings.IsAuthEnabled) {
                FirebaseAuthImplementation.Initialize(app, options, settings.FacebookId, settings.FacebookAppName);
            }

            if(settings.IsCloudMessagingEnabled) {
                FirebaseCloudMessagingImplementation.Initialize();
            }

            Console.WriteLine($"Plugin.Firebase initialized with the following settings:\n{settings}");
        }
    }
}