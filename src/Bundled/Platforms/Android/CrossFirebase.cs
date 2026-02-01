using Firebase;
using Firebase.Crashlytics;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Bundled.Shared;

namespace Plugin.Firebase.Bundled.Platforms.Android;

public static class CrossFirebase
{
    public static void Initialize(
        Activity activity,
        Func<Activity> activityLocator,
        CrossFirebaseSettings settings,
        FirebaseOptions firebaseOptions = null,
        string name = null)
    {
        Core.Platforms.Android.CrossFirebase.Initialize(
            activity,
            activityLocator,
            firebaseOptions,
            name);

        if(settings.IsAnalyticsEnabled) {
            FirebaseAnalyticsImplementation.Initialize(activity);
        }

        FirebaseCrashlytics.Instance.SetCrashlyticsCollectionEnabled(settings.IsCrashlyticsEnabled);

        Console.WriteLine($"Plugin.Firebase initialized with the following settings:\n{settings}");
    }
}