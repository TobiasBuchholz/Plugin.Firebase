using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Analytics;
using CoreCrossFirebase = Plugin.Firebase.Core.Platforms.Android.CrossFirebase;
using NativeFirebaseApp = Firebase.FirebaseApp;
using NativeFirebaseOptions = Firebase.FirebaseOptions;

namespace Plugin.Firebase.IntegrationTests;

internal static partial class FirebaseTestHost
{
    private static partial void ConfigureFirebaseLifecycleEvents(ILifecycleBuilder events)
    {
        events.AddAndroid(android => android.OnCreate((activity, _) => {
            if(IntegrationTestEnvironment.UsesEmulatorBackend) {
                DeleteDefaultFirebaseAppIfInitialized();
            }

            ConfigureAppCheckBeforeInitialize();
            CoreCrossFirebase.Initialize(
                activity,
                () => Platform.CurrentActivity!,
                IntegrationTestEnvironment.UsesEmulatorBackend ? CreateEmulatorFirebaseOptions() : null);

            if(CoreCrossFirebase.TryGetDefaultApp(out var defaultApp) && defaultApp != null) {
                if(IntegrationTestEnvironment.UsesRealBackend) {
                    // Android Analytics throws until it is initialized with an activity.
                    FirebaseAnalyticsImplementation.Initialize(activity);
                }

                ConfigureCollectionAfterInitialize();
            }

            ConfigureEmulatorsIfRequested();
        }));
    }

    private static void DeleteDefaultFirebaseAppIfInitialized()
    {
        try {
            NativeFirebaseApp.Instance.Delete();
        } catch(Java.Lang.IllegalStateException) {
            // FirebaseInitProvider only creates a default app when google-services.json is present.
        }
    }

    private static NativeFirebaseOptions CreateEmulatorFirebaseOptions()
    {
        return new NativeFirebaseOptions.Builder()
            .SetApiKey(IntegrationTestEnvironment.ApiKey)
            .SetApplicationId(IntegrationTestEnvironment.AndroidGoogleAppId)
            .SetDatabaseUrl(IntegrationTestEnvironment.DatabaseUrl)
            .SetGcmSenderId(IntegrationTestEnvironment.GcmSenderId)
            .SetProjectId(IntegrationTestEnvironment.ProjectId)
            .SetStorageBucket(IntegrationTestEnvironment.StorageBucket)
            .Build();
    }
}