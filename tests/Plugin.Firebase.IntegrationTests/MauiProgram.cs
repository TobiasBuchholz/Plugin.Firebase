using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Bundled.Shared;
#if IOS
using Plugin.Firebase.Bundled.Platforms.iOS;
#else 
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
                CrossFirebase.Initialize(CreateCrossFirebaseSettings());
                return false;
            }));
#else
            events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity, CreateCrossFirebaseSettings())));
#endif
        });
        return builder;
    }

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
            googleRequestIdToken: "316652897245-lbddc4dc4v87nv3n19thi032n3dvrcvu.apps.googleusercontent.com");
    }
}
