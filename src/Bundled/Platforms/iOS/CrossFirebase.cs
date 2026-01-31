using Firebase.Core;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Crashlytics;

namespace Plugin.Firebase.Bundled.Platforms.iOS;

/// <summary>
/// iOS-specific bundled Firebase initialization entry point.
/// </summary>
public static class CrossFirebase
{
    /// <summary>
    /// Initializes Firebase with all configured services on iOS.
    /// </summary>
    /// <param name="settings">The bundled settings specifying which services to enable.</param>
    /// <param name="firebaseOptions">Optional Firebase configuration options.</param>
    /// <param name="name">Optional name for the Firebase app instance.</param>
    public static void Initialize(
        CrossFirebaseSettings settings,
        Options firebaseOptions = null,
        string name = null
    )
    {
        if(firebaseOptions == null) {
            App.Configure();
        } else if(name == null) {
            App.Configure(firebaseOptions);
        } else {
            App.Configure(name, firebaseOptions);
        }

        if(settings.IsCloudMessagingEnabled) {
            FirebaseCloudMessagingImplementation.Initialize();
        }

        CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(
            settings.IsCrashlyticsEnabled
        );

        Console.WriteLine($"Plugin.Firebase initialized with the following settings:\n{settings}");
    }
}