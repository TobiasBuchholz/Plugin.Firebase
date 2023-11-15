using CommunityToolkit.Maui;
using Genesis.Logging;
using Microsoft.Maui.LifecycleEvents;
using Playground.Common.Services.Auth;
using Playground.Common.Services.DynamicLink;
using Playground.Common.Services.Logging;
using Playground.Common.Services.PushNotification;
using Playground.Features.Auth;
using Playground.Features.CloudMessaging;
using Playground.Features.Dashboard;
using Playground.Features.RemoteConfig;
using Playground.Features.Storage;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Auth.Facebook;
using Plugin.Firebase.Auth.Google;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.DynamicLinks;
using Plugin.Firebase.Functions;
using Plugin.Firebase.RemoteConfig;
using Plugin.Firebase.Bundled.Shared;
#if IOS
using Plugin.Firebase.Bundled.Platforms.iOS;
using Playground.Platforms.iOS.Services.UserInteraction;
#else
using Plugin.Firebase.Bundled.Platforms.Android;
using Playground.Platforms.Android.Services.UserInteraction;
#endif
using Plugin.Firebase.Storage;

namespace Playground;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        LogOutputService.Initialize();

        return MauiApp
            .CreateBuilder()
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
            .RegisterViews()
            .Build();
    }

    private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IDynamicLinkService, DynamicLinkService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IPreferencesService, PreferencesService>();
        builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();
        builder.Services.AddSingleton<ISchedulerService, SchedulerService>();
        builder.Services.AddSingleton<IUserInteractionService, UserInteractionService>();
        builder.Services.AddSingleton(LoggerService.GetLogger(typeof(MauiProgram)));
        return builder;
    }

    private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
    {
        builder.ConfigureLifecycleEvents(events => {
#if IOS
            events.AddiOS(iOS => iOS.WillFinishLaunching((app, launchOptions) => {
                CrossFirebase.Initialize(CreateCrossFirebaseSettings());
                FirebaseAuthFacebookImplementation.Initialize(app, launchOptions, "151743924915235", "Plugin Firebase Playground");
                FirebaseAuthGoogleImplementation.Initialize();
                return false;
            }));
#else
            events.AddAndroid(android => android.OnCreate((activity, _) => {
                var settings = CreateCrossFirebaseSettings();
                CrossFirebase.Initialize(activity, settings);
                FirebaseAuthGoogleImplementation.Initialize(settings.GoogleRequestIdToken);
            }));
#endif
        });

        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseAuthFacebook.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseAuthGoogle.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseCloudMessaging.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseDynamicLinks.Current);
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
            googleRequestIdToken: "537235599720-723cgj10dtm47b4ilvuodtp206g0q0fg.apps.googleusercontent.com");
    }
}
