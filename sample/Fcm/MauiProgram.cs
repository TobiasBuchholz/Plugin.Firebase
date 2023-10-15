using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.CloudMessaging;
#if ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#elif IOS
using Plugin.Firebase.Core.Platforms.iOS;
#endif

namespace Playground.FcmDemo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts => {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .RegisterFirebaseServices();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        #region Firebase

        private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
        {
            try 
            {
                builder.ConfigureLifecycleEvents(events =>
                {
#if ANDROID
                    events.AddAndroid(android => android.OnCreate((activity, _) =>
                        CrossFirebase.Initialize(activity)));
#elif IOS
                    events.AddiOS(iOS => iOS.FinishedLaunching((_, __) => {
                        CrossFirebase.Initialize();
                        FirebaseCloudMessagingImplementation.Initialize();

                        return false;
                    }));
#endif
                });

                builder.Services.AddSingleton(_ => CrossFirebaseCloudMessaging.Current);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error trying to init Firebase Messaging\n{ex}");
            }

            return builder;
        }

        #endregion
    }
}
