using Foundation;
using Microsoft.Maui.LifecycleEvents;
using NativeFirebaseOptions = Firebase.Core.Options;

namespace Plugin.Firebase.IntegrationTests;

internal static partial class FirebaseTestHost
{
    private static partial void ConfigureFirebaseLifecycleEvents(ILifecycleBuilder events)
    {
        events.AddiOS(iOS => iOS.WillFinishLaunching((_, _) => {
            if(IntegrationTestEnvironment.UsesRealBackend) {
                EnsureFirebaseConfigPresent();
                InitializeBundledFirebase(CreateCrossFirebaseSettings());
            } else {
                InitializeBundledFirebase(
                    CreateCrossFirebaseSettings(),
                    CreateEmulatorFirebaseOptions());
            }

            ConfigureEmulatorsIfRequested();
            return false;
        }));
    }

    [System.Diagnostics.CodeAnalysis.DynamicDependency(
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicMethods,
        "Plugin.Firebase.Bundled.Platforms.iOS.CrossFirebase",
        "Plugin.Firebase")]
    private static void InitializeBundledFirebase(
        Plugin.Firebase.Bundled.Shared.CrossFirebaseSettings settings,
        NativeFirebaseOptions? firebaseOptions = null)
    {
        const string initializerTypeName = "Plugin.Firebase.Bundled.Platforms.iOS.CrossFirebase";

        var initializerType = typeof(Plugin.Firebase.Bundled.Shared.CrossFirebaseSettings)
            .Assembly
            .GetType(initializerTypeName, throwOnError: true);
        if(initializerType == null) {
            throw new InvalidOperationException($"Unable to find type '{initializerTypeName}'.");
        }
        var initialize = initializerType.GetMethod(
            "Initialize",
            [
                typeof(Plugin.Firebase.Bundled.Shared.CrossFirebaseSettings),
                typeof(NativeFirebaseOptions),
                typeof(string)
            ]);

        if(initialize == null) {
            throw new MissingMethodException(
                initializerTypeName,
                "Initialize(CrossFirebaseSettings, Firebase.Core.Options, string)");
        }

        initialize.Invoke(null, [settings, firebaseOptions, null]);
    }

    private static void EnsureFirebaseConfigPresent()
    {
        var bundleIdentifier = NSBundle.MainBundle.BundleIdentifier;
        var configPath = NSBundle.MainBundle.PathForResource("GoogleService-Info", "plist");

        if(configPath == null) {
            throw new InvalidOperationException(
                "GoogleService-Info.plist was not bundled into the integration test app. "
                    + "Place the file at tests/Plugin.Firebase.IntegrationTests/GoogleService-Info.plist "
                    + $"and make sure it was generated for the bundle identifier '{bundleIdentifier}'."
            );
        }

        var config = NSMutableDictionary.FromFile(configPath);
        var configuredBundleIdentifier = config.ObjectForKey(new NSString("BUNDLE_ID"))?.ToString();
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
}