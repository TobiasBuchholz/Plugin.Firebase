using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Plugin.Firebase.CloudMessaging;

namespace Playground.FcmDemo
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HandleIntent(Intent);

            CreateNotificationChannel();
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            HandleIntent(intent);
        }

        private static void HandleIntent(Intent intent)
        {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }

        private void CreateNotificationChannel()
        {
            Console.WriteLine($"Android Sdk ={Build.VERSION.SdkInt}");

            try {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) {
#pragma warning disable CA1416 // Validate platform compatibility
                    Console.WriteLine($"Check Permission {Manifest.Permission.PostNotifications}");

                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) != Permission.Granted)
                    {
                        Console.WriteLine($"Request Permission {Manifest.Permission.PostNotifications}");
                        ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.PostNotifications }, 0);
                    }
#pragma warning restore CA1416 // Validate platform compatibility
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error trying to get Notification permission\n{ex}");
            }

            try
            {
                Console.WriteLine($"Init FCM Channel");
                var channelId = $"{PackageName}.general";
                var notificationManager = (NotificationManager) GetSystemService(NotificationService);
                var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
                notificationManager.CreateNotificationChannel(channel);
                FirebaseCloudMessagingImplementation.ChannelId = channelId;
                FirebaseCloudMessagingImplementation.SmallIconRef = Resource.Drawable.ic_notification_small; // INFO: This is the message Icon displayed on Android
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error trying to create notification channel\n{ex}");
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
