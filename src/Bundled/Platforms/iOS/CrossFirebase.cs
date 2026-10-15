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
    /// <remarks>
    /// App Check options that can't be applied are logged instead of thrown, and the App Check provider stays as it was:
    /// a provider iOS doesn't support, or, once the default Firebase app is configured, any provider other than the one
    /// the plugin installed, for example when this runs again for a secondary app.
    /// </remarks>
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
            } catch(NotSupportedException e) {
                // A provider configured earlier, or the native default (DeviceCheck), stays in place.
                Console.WriteLine($"Plugin.Firebase AppCheck: {e.Message} Continuing without changing the App Check provider.");
            } catch(InvalidOperationException e) when(e is not ObjectDisposedException) {
                // The native SDK reads the provider factory only when the default app is configured, so it can't be
                // changed once the default app exists.
                Console.WriteLine($"Plugin.Firebase AppCheck: {e.Message} Continuing without changing the App Check provider.");
            }
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