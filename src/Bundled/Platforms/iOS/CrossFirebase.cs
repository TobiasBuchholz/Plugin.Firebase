using Firebase.Core;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Crashlytics;
using Plugin.Firebase.Bundled.Shared;

namespace Plugin.Firebase.Bundled.Platforms.iOS;

public static class CrossFirebase
{
    public static void Initialize(
        CrossFirebaseSettings settings,
        Options firebaseOptions = null,
        string name = null)
    {
        if(settings.AppCheckOptions != null) {
            try {
                CrossFirebaseAppCheck.Configure(settings.AppCheckOptions);
            } catch(NotSupportedException) {
                Console.WriteLine("Plugin.Firebase AppCheck is not supported for this iOS build. Continuing without AppCheck.");
            }
        }

        Core.Platforms.iOS.CrossFirebase.Initialize(name, firebaseOptions);

        if(settings.IsCloudMessagingEnabled) {
            FirebaseCloudMessagingImplementation.Initialize();
        }

        CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(settings.IsCrashlyticsEnabled);

        Console.WriteLine($"Plugin.Firebase initialized with the following settings:\n{settings}");
    }
}