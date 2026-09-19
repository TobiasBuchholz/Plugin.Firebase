using Firebase.Core;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Crashlytics;
using Plugin.Firebase.PerformanceMonitoring;
using NativePerformance = Firebase.PerformanceMonitoring.Performance;

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
        if(settings.AppCheckOptions != null) {
            try {
                CrossFirebaseAppCheck.Configure(settings.AppCheckOptions);
            } catch(NotSupportedException) {
                Console.WriteLine("Plugin.Firebase AppCheck is not supported for this iOS build. Continuing without AppCheck.");
            }
        } else {
            // The bundled package links the native App Check framework, which defaults to DeviceCheck. Creating the App
            // Check implementation registers the hook that installs the documented Disabled default before Firebase
            // configures; options already passed to CrossFirebaseAppCheck.Configure are kept.
            _ = CrossFirebaseAppCheck.IsSupported;
        }

        NativePerformance.SharedInstance.DataCollectionEnabled =
            settings.IsPerformanceMonitoringEnabled;
        NativePerformance.SharedInstance.InstrumentationEnabled =
            settings.IsPerformanceMonitoringEnabled;

        Core.Platforms.iOS.CrossFirebase.Initialize(name, firebaseOptions);

        if(settings.IsCloudMessagingEnabled) {
            FirebaseCloudMessagingImplementation.Initialize();
        }

        CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(
            settings.IsCrashlyticsEnabled
        );
        CrossFirebasePerformanceMonitoring.Current.IsDataCollectionEnabled =
            settings.IsPerformanceMonitoringEnabled;

        Console.WriteLine($"Plugin.Firebase initialized with the following settings:\n{settings}");
    }
}