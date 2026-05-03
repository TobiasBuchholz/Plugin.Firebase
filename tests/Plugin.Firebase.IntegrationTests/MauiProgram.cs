using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.Firestore;
using Plugin.Firebase.Functions;
using Plugin.Firebase.IntegrationTests;
using Plugin.Firebase.Storage;
#if IOS
using Foundation;
using NativeFirebaseOptions = Firebase.Core.Options;
using PlatformCrossFirebase = Plugin.Firebase.Bundled.Platforms.iOS.CrossFirebase;
#elif ANDROID
using NativeFirebaseApp = global::Firebase.FirebaseApp;
using NativeFirebaseOptions = Firebase.FirebaseOptions;
using PlatformCrossFirebase = Plugin.Firebase.Bundled.Platforms.Android.CrossFirebase;
#endif
using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;
using DeviceRunners.XHarness;

namespace Plugin.Firebase.IntegrationTests2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var useVisualRunner = IntegrationTestEnvironment.IsFeatureEnabled(
            "PLUGIN_FIREBASE_USE_VISUAL_RUNNER",
            "debug.pluginfirebase.visual.use");
        var builder = MauiApp
            .CreateBuilder()
            .ConfigureUITesting()
            .UseVisualTestRunner(conf => {
                if(useVisualRunner) {
                    conf.SetTestRunnerUsage(VisualTestRunnerUsage.Always);
                } else {
                    conf.SetTestRunnerUsage(VisualTestRunnerUsage.Never);
                }

                conf.AddConsoleResultChannel()
                    .AddTestAssembly(typeof(MauiProgram).Assembly)
                    .AddXunit();
            })
            .UseXHarnessTestRunner(conf => {
                if(useVisualRunner) {
                    conf.SetTestRunnerUsage(XHarnessTestRunnerUsage.Never);
                } else {
                    conf.SetTestRunnerUsage(XHarnessTestRunnerUsage.Always);
                }

                conf.AddTestAssembly(typeof(MauiProgram).Assembly)
                    .AddXunit();
            })
            .RegisterFirebaseServices();

        return builder.Build();
    }

    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.WillFinishLaunching((_,__) => {
                if(IntegrationTestEnvironment.UsesRealBackend) {
                    EnsureFirebaseConfigPresent();
                    PlatformCrossFirebase.Initialize(CreateCrossFirebaseSettings());
                } else {
                    PlatformCrossFirebase.Initialize(
                        CreateCrossFirebaseSettings(),
                        CreateEmulatorFirebaseOptions());
                }
                ConfigureEmulatorsIfRequested();
                return false;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) => {
                if(IntegrationTestEnvironment.UsesEmulatorBackend) {
                    DeleteDefaultFirebaseAppIfInitialized();
                }

                PlatformCrossFirebase.Initialize(
                    activity,
                    () => Platform.CurrentActivity,
                    CreateCrossFirebaseSettings(),
                    IntegrationTestEnvironment.UsesEmulatorBackend ? CreateEmulatorFirebaseOptions() : null);
                ConfigureEmulatorsIfRequested();
            }));
#endif
        });
        return builder;
    }

#if IOS
    private static void EnsureFirebaseConfigPresent()
    {
        var bundleIdentifier = NSBundle.MainBundle.BundleIdentifier ?? "<unknown>";
        var configPath = NSBundle.MainBundle.PathForResource("GoogleService-Info", "plist");

        if(configPath == null) {
            throw new InvalidOperationException(
                "GoogleService-Info.plist was not bundled into the integration test app. "
                    + "Place the file at tests/Plugin.Firebase.IntegrationTests/GoogleService-Info.plist "
                    + $"and make sure it was generated for the bundle identifier '{bundleIdentifier}'."
            );
        }

        var config = NSMutableDictionary.FromFile(configPath);
        var configuredBundleIdentifier = config?.ObjectForKey(new NSString("BUNDLE_ID"))?.ToString();
        if(string.IsNullOrWhiteSpace(configuredBundleIdentifier)) {
            throw new InvalidOperationException(
                "GoogleService-Info.plist is missing the BUNDLE_ID entry. "
                    + "Regenerate the plist from Firebase and bundle it into the integration test app."
            );
        }

        if(!string.Equals(configuredBundleIdentifier, bundleIdentifier, StringComparison.Ordinal)) {
            throw new InvalidOperationException(
                $"GoogleService-Info.plist was generated for '{configuredBundleIdentifier}', "
                    + $"but the integration test app is running as '{bundleIdentifier}'. "
                    + "Update the app bundle identifier or replace the plist so they match."
            );
        }
    }
#endif

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

#if IOS
    private static NativeFirebaseOptions CreateEmulatorFirebaseOptions()
    {
        return new NativeFirebaseOptions(
            IntegrationTestEnvironment.IosGoogleAppId,
            IntegrationTestEnvironment.GcmSenderId) {
            ApiKey = IntegrationTestEnvironment.ApiKey,
            BundleId = NSBundle.MainBundle.BundleIdentifier,
            DatabaseUrl = IntegrationTestEnvironment.DatabaseUrl,
            ProjectId = IntegrationTestEnvironment.ProjectId,
            StorageBucket = IntegrationTestEnvironment.StorageBucket
        };
    }
#elif ANDROID
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
#endif
}