using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Analytics;
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
            events.AddiOS(iOS => iOS.FinishedLaunching((_,__) => {
                CrossFirebase.Initialize();
                return false;
            }));
#else
            events.AddAndroid(android => android.OnCreate((activity, _) => {
                CrossFirebase.Initialize(activity);
                FirebaseAnalyticsImplementation.Initialize(activity);
            }));
            
#endif
        });
        return builder;
    }
}