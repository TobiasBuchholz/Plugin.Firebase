using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Genesis.Logging;
using Microsoft.Maui.LifecycleEvents;
using Playground.Common.Services.Auth;
using Playground.Common.Services.Navigation;
using Playground.Common.Services.Preferences;
using Playground.Common.Services.PushNotification;
using Playground.Common.Services.Scheduler;
using Playground.Common.Services.UserInteraction;
using Playground.Features.Auth;
using Playground.Features.CloudMessaging;
using Playground.Features.Dashboard;
using Playground.Features.RemoteConfig;
using Playground.Features.Storage;
using Plugin.Firebase.Auth;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Functions;
using Plugin.Firebase.RemoteConfig;
#if IOS
using Plugin.Firebase.iOS;
using Playground.Platforms.iOS.Services.UserInteraction;
#else
using Plugin.Firebase.Android;
using Playground.Platforms.Android.Services.UserInteraction;
using Plugin.CurrentActivity;
#endif
using Plugin.Firebase.Shared;
using Plugin.Firebase.Storage;

namespace Playground;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .RegisterServices()
            .RegisterFirebaseServices()
            .RegisterViewModels()
            .RegisterViews();

        return builder.Build();
    }

    private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
		builder.Services.AddSingleton<IAuthService, AuthService>();
		builder.Services.AddSingleton<INavigationService, NavigationService>();
		builder.Services.AddSingleton<IPreferencesService, PreferencesService>();
		builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();
		builder.Services.AddSingleton<ISchedulerService, SchedulerService>();
		builder.Services.AddSingleton<IUserInteractionService, UserInteractionService>();

        builder.Services.AddSingleton(LoggerService.GetLogger(typeof(MauiProgram)));
#if ANDROID
        builder.Services.AddSingleton(_ => CrossCurrentActivity.Current);
#endif
        return builder;
    }

    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.FinishedLaunching((app, launchOptions) => {
                CrossFirebase.Initialize(app, launchOptions, CreateCrossFirebaseSettings());
                return false;
            }));
#else
            events.AddAndroid(android => android.OnCreate((activity, state) =>
                CrossFirebase.Initialize(activity, state, CreateCrossFirebaseSettings())));
#endif
        });
        
        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseCloudMessaging.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseFunctions.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseStorage.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseRemoteConfig.Current);
        return builder;
    }
    
    private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<DashboardViewModel>();
		builder.Services.AddTransient<AuthViewModel>();
		builder.Services.AddTransient<CloudMessagingViewModel>();
		builder.Services.AddTransient<RemoteConfigViewModel>();
		builder.Services.AddTransient<StorageViewModel>();
        return builder;
    }
    
    private static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
		builder.Services.AddSingleton<DashboardPage>();
		builder.Services.AddTransient<AuthPage>();
		builder.Services.AddTransient<CloudMessagingPage>();
		builder.Services.AddTransient<RemoteConfigPage>();
		builder.Services.AddTransient<StoragePage>();
        return builder;
    }

    private static CrossFirebaseSettings CreateCrossFirebaseSettings()
    {
        return new CrossFirebaseSettings(
            isAnalyticsEnabled: true,
            isAuthEnabled: true,
            isCloudMessagingEnabled: true,
            isDynamicLinksEnabled: true,
            isFirestoreEnabled: true,
            isFunctionsEnabled: true,
            isRemoteConfigEnabled: true,
            isStorageEnabled: true,
            facebookId: "151743924915235",
            facebookAppName: "Plugin Firebase Playground",
            googleRequestIdToken: "537235599720-723cgj10dtm47b4ilvuodtp206g0q0fg.apps.googleusercontent.com");
    }
}

