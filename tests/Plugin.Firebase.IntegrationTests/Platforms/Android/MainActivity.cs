using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.Firebase.CloudMessaging;

namespace Plugin.Firebase.IntegrationTests;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CreateNotificationChannel();
        var intent = Intent;
        if(intent != null) {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        if(intent != null) {
            FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }
    }

    private void CreateNotificationChannel()
    {
        if(!OperatingSystem.IsAndroidVersionAtLeast(26)) {
            return;
        }

        var channelId = $"{PackageName}.general";
        if(GetSystemService(NotificationService) is not NotificationManager notificationManager) {
            throw new InvalidOperationException("Unable to get NotificationManager.");
        }
        var channel = new NotificationChannel(channelId, "General", NotificationImportance.Default);
        notificationManager.CreateNotificationChannel(channel);
        FirebaseCloudMessagingImplementation.ChannelId = channelId;
    }
}