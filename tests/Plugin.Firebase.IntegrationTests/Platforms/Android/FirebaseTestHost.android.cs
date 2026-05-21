using Microsoft.Maui.LifecycleEvents;
using NativeFirebaseApp = Firebase.FirebaseApp;
using NativeFirebaseOptions = Firebase.FirebaseOptions;
using PlatformCrossFirebase = Plugin.Firebase.Bundled.Platforms.Android.CrossFirebase;

namespace Plugin.Firebase.IntegrationTests;

internal static partial class FirebaseTestHost
{
    private static partial void ConfigureFirebaseLifecycleEvents(ILifecycleBuilder events)
    {
        events.AddAndroid(android => android.OnCreate((activity, _) => {
            if(IntegrationTestEnvironment.UsesEmulatorBackend) {
                DeleteDefaultFirebaseAppIfInitialized();
            }

            PlatformCrossFirebase.Initialize(
                activity,
                () => Platform.CurrentActivity!,
                CreateCrossFirebaseSettings(),
                IntegrationTestEnvironment.UsesEmulatorBackend ? CreateEmulatorFirebaseOptions() : null);
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