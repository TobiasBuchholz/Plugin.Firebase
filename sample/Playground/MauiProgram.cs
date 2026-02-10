using CommunityToolkit.Maui;
using Genesis.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Storage;
using Playground.Common.Services.Auth;
using Playground.Common.Services.Logging;
using Playground.Common.Services.PushNotification;
using Playground.Common.Services.Preferences;
using Playground.Features.AppCheck;
using Playground.Features.Auth;
using Playground.Features.CloudMessaging;
using Playground.Features.Dashboard;
using Playground.Features.RemoteConfig;
using Playground.Features.Storage;
using Plugin.Firebase.AppCheck;
using Plugin.Firebase.Auth;
using Plugin.Firebase.Bundled.Shared;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Functions;
using Plugin.Firebase.RemoteConfig;
using Plugin.Firebase.Storage;
#if IOS
using Plugin.Firebase.Bundled.Platforms.iOS;
using Playground.Platforms.iOS.Services.UserInteraction;
#elif ANDROID
using Plugin.Firebase.Bundled.Platforms.Android;
using Playground.Platforms.Android.Services.UserInteraction;
#endif

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
            events.AddiOS(iOS =>
                iOS.WillFinishLaunching(
                    (app, launchOptions) =>
                    {
                        CrossFirebase.Initialize(CreateCrossFirebaseSettings());
                        return false;
                    }
                )
            );
#elif ANDROID
            events.AddAndroid(android =>
                android.OnCreate(
                    (activity, _) => {
                        var settings = CreateCrossFirebaseSettings();
                        CrossFirebase.Initialize(
                            activity,
                            () => Platform.CurrentActivity,
                            settings
                        );
                    }
                )
            );
#endif
        });

        builder.Services.AddSingleton(_ => CrossFirebaseAuth.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseCloudMessaging.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseFunctions.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseStorage.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseRemoteConfig.Current);
        builder.Services.AddSingleton(_ => CrossFirebaseAppCheck.Current);
        builder.Services.AddSingleton(_ => GetConfiguredAppCheckOptions());
        return builder;
    }

    private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<DashboardViewModel>();
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<AppCheckViewModel>();
        builder.Services.AddTransient<AppCheckModeSelectionViewModel>();
        builder.Services.AddTransient<CloudMessagingViewModel>();
        builder.Services.AddTransient<RemoteConfigViewModel>();
        builder.Services.AddTransient<StorageViewModel>();
        return builder;
    }

    private static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddTransient<AuthPage>();
        builder.Services.AddTransient<AppCheckPage>();
        builder.Services.AddTransient<AppCheckModeSelectionPage>();
        builder.Services.AddTransient<CloudMessagingPage>();
        builder.Services.AddTransient<RemoteConfigPage>();
        builder.Services.AddTransient<StoragePage>();
        return builder;
    }

    private static AppCheckOptions GetConfiguredAppCheckOptions()
    {
        var modeString = Preferences.Get(PreferenceKeys.AppCheckMode, "Debug");
        return modeString switch {
            "Disabled" => AppCheckOptions.Disabled,
            "Device Check" => AppCheckOptions.DeviceCheck,
            "App Attest" => AppCheckOptions.AppAttest,
            _ => AppCheckOptions.Debug // Default to Debug
        };
    }

    private static CrossFirebaseSettings CreateCrossFirebaseSettings()
    {
        var appCheckOptions = GetConfiguredAppCheckOptions();
        return new CrossFirebaseSettings(
            isAnalyticsEnabled: true,
            isAuthEnabled: true,
            isCloudMessagingEnabled: true,
            isDynamicLinksEnabled: true,
            isFirestoreEnabled: true,
            isFunctionsEnabled: true,
            isRemoteConfigEnabled: true,
            isStorageEnabled: true,
            appCheckOptions: appCheckOptions,
            googleRequestIdToken: "537235599720-723cgj10dtm47b4ilvuodtp206g0q0fg.apps.googleusercontent.com"
        );
    }
}