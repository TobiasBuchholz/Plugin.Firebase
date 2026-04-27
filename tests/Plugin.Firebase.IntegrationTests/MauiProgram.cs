using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.Functions;
using Plugin.Firebase.Storage;
#if IOS
using Foundation;
using Plugin.Firebase.Bundled.Platforms.iOS;
#elif ANDROID
using AndroidRuntime = Android.Runtime;
using Plugin.Firebase.Bundled.Platforms.Android;
#endif
using Xunit.Runners.Maui;

namespace Plugin.Firebase.IntegrationTests2;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return MauiApp
            .CreateBuilder()
            .ConfigureTests(new TestOptions { Assemblies = { typeof(MauiProgram).Assembly } })
            .RegisterFirebaseServices()
            .UseVisualRunner()
            .Build();
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
        return string.Equals(
            GetConfigurationValue(environmentVariableName, androidSystemPropertyName),
            "1",
            StringComparison.Ordinal);
    }

    private static string GetEmulatorHost(string environmentVariableName, string androidSystemPropertyName)
    {
        var host = GetConfigurationValue(environmentVariableName, androidSystemPropertyName);
        return string.IsNullOrWhiteSpace(host)
            ? OperatingSystem.IsAndroid() ? "10.0.2.2" : "localhost"
            : host;
    }

    private static int GetEmulatorPort(string environmentVariableName, string androidSystemPropertyName, int defaultPort)
    {
        var portValue = GetConfigurationValue(environmentVariableName, androidSystemPropertyName);
        if(string.IsNullOrWhiteSpace(portValue)) {
            return defaultPort;
        }

        if(!int.TryParse(portValue, out var port)) {
            throw new InvalidOperationException(
                $"{environmentVariableName} must be an integer, but was '{portValue}'.");
        }

        return port;
    }

    private static string GetConfigurationValue(string environmentVariableName, string androidSystemPropertyName)
    {
        var environmentVariableValue = Environment.GetEnvironmentVariable(environmentVariableName);
        if(!string.IsNullOrWhiteSpace(environmentVariableValue)) {
            return environmentVariableValue;
        }

#if ANDROID
        return GetAndroidSystemProperty(androidSystemPropertyName);
#else
        return null;
#endif
    }

#if ANDROID
    private static string GetAndroidSystemProperty(string propertyName)
    {
        IntPtr? propertyValuePointer = null;

        try {
            var systemPropertiesClass = AndroidRuntime.JNIEnv.FindClass("android/os/SystemProperties");
            var getMethodId = AndroidRuntime.JNIEnv.GetStaticMethodID(
                systemPropertiesClass,
                "get",
                "(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;");

            using var propertyNameValue = new Java.Lang.String(propertyName);
            using var defaultValue = new Java.Lang.String(string.Empty);
            propertyValuePointer = AndroidRuntime.JNIEnv.CallStaticObjectMethod(
                systemPropertiesClass,
                getMethodId,
                new AndroidRuntime.JValue(propertyNameValue),
                new AndroidRuntime.JValue(defaultValue));

            return AndroidRuntime.JNIEnv.GetString(
                propertyValuePointer.Value,
                AndroidRuntime.JniHandleOwnership.DoNotTransfer);
        } catch {
            return null;
        } finally {
            if(propertyValuePointer.HasValue && propertyValuePointer.Value != IntPtr.Zero) {
                AndroidRuntime.JNIEnv.DeleteLocalRef(propertyValuePointer.Value);
            }
        }
    }
#endif
}
