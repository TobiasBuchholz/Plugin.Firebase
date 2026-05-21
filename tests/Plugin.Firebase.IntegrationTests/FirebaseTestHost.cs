using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Functions;
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

    private static CrossFirebaseSettings CreateCrossFirebaseSettings()
    {
        if(IntegrationTestEnvironment.UsesEmulatorBackend) {
            return new CrossFirebaseSettings(
                isAuthEnabled: true,
                isFirestoreEnabled: true,
                isFunctionsEnabled: true,
                isStorageEnabled: true,
                appCheckOptions: AppCheckOptions.Disabled) {
                IsInstallationsEnabled = true,
                IsPerformanceMonitoringEnabled = true
            };
        }

        return new CrossFirebaseSettings(
            isAnalyticsEnabled: true,
            isAuthEnabled: true,
            isCloudMessagingEnabled: true,
            isCrashlyticsEnabled: true,
            isDynamicLinksEnabled: true,
            isFirestoreEnabled: true,
            isFunctionsEnabled: true,
            isRemoteConfigEnabled: true,
            isStorageEnabled: true,
            appCheckOptions: IntegrationTestEnvironment.ShouldRunAppCheckTokenTests
                ? AppCheckOptions.Debug
                : AppCheckOptions.Disabled) {
            IsInstallationsEnabled = true,
            IsPerformanceMonitoringEnabled = true
        };
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