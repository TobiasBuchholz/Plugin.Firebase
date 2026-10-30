using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Crashlytics;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Functions;
using Plugin.Firebase.PerformanceMonitoring;
using Plugin.Firebase.Storage;

namespace Plugin.Firebase.IntegrationTests;

internal static partial class FirebaseTestHost
{
    public static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(ConfigureFirebaseLifecycleEvents);
        return builder;
    }

    private static partial void ConfigureFirebaseLifecycleEvents(ILifecycleBuilder events);

    /// <summary>
    /// Installs the App Check provider the run needs. This has to happen before Firebase configures: on iOS a
    /// provider factory installed later never takes effect, and the app links App Check, so leaving it unconfigured
    /// would hand every request to the native DeviceCheck default.
    /// </summary>
    private static void ConfigureAppCheckBeforeInitialize()
    {
        CrossFirebaseAppCheck.Configure(
            IntegrationTestEnvironment.UsesRealBackend && IntegrationTestEnvironment.ShouldRunAppCheckTokenTests
                ? AppCheckOptions.Debug
                : AppCheckOptions.Disabled);
    }

    /// <summary>
    /// Applies the collection settings the suite expects once Firebase is initialized. Crash reports are only sent on
    /// the real backend. Performance collection is switched on explicitly, as the bundled host did: the flag persists,
    /// so this clears a disable left by an interrupted run, and the real-backend tests send their traces.
    /// </summary>
    private static void ConfigureCollectionAfterInitialize()
    {
        CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(IntegrationTestEnvironment.UsesRealBackend);
        CrossFirebasePerformanceMonitoring.Current.IsDataCollectionEnabled = true;
    }

    private static void ConfigureEmulatorsIfRequested()
    {
        if(IntegrationTestEnvironment.ShouldUseAuthEmulator) {
            var auth = IntegrationTestEnvironment.AuthEmulatorEndpoint;
            CrossFirebaseAuth.Current.UseEmulator(auth.Host, auth.Port);
        }

        if(IntegrationTestEnvironment.ShouldUseFirestoreEmulator) {
            var firestore = IntegrationTestEnvironment.FirestoreEmulatorEndpoint;
            CrossFirebaseFirestore.Current.UseEmulator(firestore.Host, firestore.Port);
        }

        if(IntegrationTestEnvironment.ShouldUseFunctionsEmulator) {
            var functions = IntegrationTestEnvironment.FunctionsEmulatorEndpoint;
            CrossFirebaseFunctions.Current.UseEmulator(functions.Host, functions.Port);
        }

        if(IntegrationTestEnvironment.ShouldUseStorageEmulator) {
            var storage = IntegrationTestEnvironment.StorageEmulatorEndpoint;
            CrossFirebaseStorage.Current.UseEmulator(storage.Host, storage.Port);
        }
    }
}