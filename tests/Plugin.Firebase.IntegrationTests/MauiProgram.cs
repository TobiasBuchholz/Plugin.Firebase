using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.Functions;
using Plugin.Firebase.IntegrationTests;
using Plugin.Firebase.Storage;
#if IOS
using Foundation;
using Plugin.Firebase.Bundled.Platforms.iOS;
#elif ANDROID
using Plugin.Firebase.Bundled.Platforms.Android;
#endif
using DeviceRunners.UITesting;
using DeviceRunners.VisualRunners;
using DeviceRunners.XHarness;

namespace Plugin.Firebase.IntegrationTests2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var useVisualRunner = IsFeatureEnabled(
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
                EnsureFirebaseConfigPresent();
                CrossFirebase.Initialize(CreateCrossFirebaseSettings());
                ConfigureFunctionsEmulatorIfRequested();
                ConfigureStorageEmulatorIfRequested();
                return false;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) => {
                CrossFirebase.Initialize(activity, () => Platform.CurrentActivity, CreateCrossFirebaseSettings());
                ConfigureFunctionsEmulatorIfRequested();
                ConfigureStorageEmulatorIfRequested();
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
            appCheckOptions: AppCheckOptions.Disabled);
    }

    private static void ConfigureFunctionsEmulatorIfRequested()
    {
        var shouldUseFunctionsEmulator = IsFeatureEnabled(
            "PLUGIN_FIREBASE_USE_FUNCTIONS_EMULATOR",
            "debug.pluginfirebase.functions.use");
        if(!shouldUseFunctionsEmulator) {
            return;
        }

        var host = GetEmulatorHost(
            "PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_HOST",
            "debug.pluginfirebase.functions.host");
        var port = GetEmulatorPort(
            "PLUGIN_FIREBASE_FUNCTIONS_EMULATOR_PORT",
            "debug.pluginfirebase.functions.port",
            5001);
        CrossFirebaseFunctions.Current.UseEmulator(host, port);
    }

    private static void ConfigureStorageEmulatorIfRequested()
    {
        var shouldUseStorageEmulator = IsFeatureEnabled(
            "PLUGIN_FIREBASE_USE_STORAGE_EMULATOR",
            "debug.pluginfirebase.storage.use");
        if(!shouldUseStorageEmulator) {
            return;
        }

        var host = GetEmulatorHost(
            "PLUGIN_FIREBASE_STORAGE_EMULATOR_HOST",
            "debug.pluginfirebase.storage.host");
        var port = GetEmulatorPort(
            "PLUGIN_FIREBASE_STORAGE_EMULATOR_PORT",
            "debug.pluginfirebase.storage.port",
            9199);
        CrossFirebaseStorage.Current.UseEmulator(host, port);
    }

    private static bool IsFeatureEnabled(string environmentVariableName, string androidSystemPropertyName)
    {
        return IntegrationTestConfiguration.IsFeatureEnabled(environmentVariableName, androidSystemPropertyName);
    }

    private static string GetEmulatorHost(string environmentVariableName, string androidSystemPropertyName)
    {
        return IntegrationTestConfiguration.GetEmulatorHost(environmentVariableName, androidSystemPropertyName);
    }

    private static int GetEmulatorPort(string environmentVariableName, string androidSystemPropertyName, int defaultPort)
    {
        return IntegrationTestConfiguration.GetEmulatorPort(
            environmentVariableName,
            androidSystemPropertyName,
            defaultPort);
    }
}
