using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Analytics;
using Plugin.Firebase.Auth;
#if IOS
using Plugin.Firebase.iOS;
#else 
using Plugin.Firebase.Android;
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
            events.AddiOS(iOS => iOS.FinishedLaunching((app, launchOptions) => {
                CrossFirebase.Initialize();
                FirebaseAuthImplementation.Initialize(app, launchOptions, "151743924915235", "Plugin Firebase Integration Tests");
                return false;
            }));
#else
            events.AddAndroid(android => android.OnCreate((activity,_) => {
                CrossFirebase.Initialize(activity);
                FirebaseAnalyticsImplementation.Initialize(activity);
                FirebaseAuthImplementation.Initialize("316652897245-lbddc4dc4v87nv3n19thi032n3dvrcvu.apps.googleusercontent.com");
            }));
#endif
        });
        return builder;
    }
}