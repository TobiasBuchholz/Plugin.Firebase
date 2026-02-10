using Firebase;
using Firebase.Crashlytics;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Bundled.Shared;

namespace Plugin.Firebase.Bundled.Platforms.Android;

/// <summary>
/// Android-specific bundled Firebase initialization entry point.
/// </summary>
public static class CrossFirebase
{
    /// <summary>
    /// Initializes Firebase with all configured services on Android.
    /// </summary>
    /// <param name="activity">The current activity.</param>
    /// <param name="activityLocator">A delegate that returns the current Android activity.</param>
    /// <param name="settings">The bundled settings specifying which services to enable.</param>
    /// <param name="firebaseOptions">Optional Firebase configuration options.</param>
    /// <param name="name">Optional name for the Firebase app instance.</param>
    public static void Initialize(
        Activity activity,
        Func<Activity> activityLocator,
        CrossFirebaseSettings settings,
        FirebaseOptions firebaseOptions = null,
        string name = null
    )
    {
        if(settings.AppCheckOptions != null) {
            try {
                CrossFirebaseAppCheck.Configure(settings.AppCheckOptions);
            } catch(NotSupportedException) {
                Console.WriteLine(
                    "Plugin.Firebase AppCheck is not supported for this build. Continuing without AppCheck."
                );
            }
        }

        Core.Platforms.Android.CrossFirebase.Initialize(
            activity,
            activityLocator,
            firebaseOptions,
            name
        );

        if(!Core.Platforms.Android.CrossFirebase.TryGetDefaultApp(out _)) {
            Console.WriteLine(
                "[Plugin.Firebase.Bundled] Skipping Analytics/Crashlytics setup: no default Firebase app. "
                    + "Ensure google-services.json is present and its package_name matches your ApplicationId."
            );
            return;
        }

        if(settings.IsAnalyticsEnabled) {
            FirebaseAnalyticsImplementation.Initialize(activity);
        }

        FirebaseCrashlytics.Instance.SetCrashlyticsCollectionEnabled(settings.IsCrashlyticsEnabled);

        Console.WriteLine($"Plugin.Firebase initialized with the following settings:\n{settings}");
    }
}