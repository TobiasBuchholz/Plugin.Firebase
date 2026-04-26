using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Bundled.Shared;
#if IOS
using Foundation;
using Plugin.Firebase.Bundled.Platforms.iOS;
#elif ANDROID
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
                return false;
            }));
#elif ANDROID
            events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity, () => Platform.CurrentActivity, CreateCrossFirebaseSettings())));
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
}
