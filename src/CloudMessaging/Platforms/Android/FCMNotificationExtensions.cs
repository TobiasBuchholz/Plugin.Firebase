using Android.Content;
using Android.OS;
using Firebase.Messaging;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.Android.Extensions;

namespace Plugin.Firebase.Android.CloudMessaging;

public static class FCMNotificationExtensions
{
    private const string BundleKeyTitle = "title";
    private const string BundleKeyBody = "body";
    private const string BundleKeyImageUrl = "image_url";
    private const string BundleKeyData = "data";

    public static FCMNotification ToFCMNotification(this RemoteMessage message)
    {
        var notification = message.GetNotification();
        return new FCMNotification(
            notification?.Body,
            notification?.Title,
            notification?.ImageUrl?.ToString(),
             message.Data);
    }

    public static FCMNotification ToFCMNotification(this Bundle bundle)
    {
        return new FCMNotification(
            bundle.GetString(BundleKeyBody),
            bundle.GetString(BundleKeyTitle),
            bundle.GetString(BundleKeyImageUrl),
            bundle.GetBundle(BundleKeyData).ToDictionary());
    }

    public static Bundle ToBundle(this FCMNotification notification)
    {
        var bundle = new Bundle();
        bundle.PutString(BundleKeyBody, notification.Body);
        bundle.PutString(BundleKeyTitle, notification.Title);
        bundle.PutString(BundleKeyImageUrl, notification.ImageUrl);
        bundle.PutBundle(BundleKeyData, notification.Data?.ToBundle());
        return bundle;
    }

    public static FCMNotification GetNotificationFromExtras(this Intent intent, string extraName)
    {
        if(intent.HasExtra(extraName)) {
            return intent
                .GetBundleExtra(extraName)
                .ToFCMNotification();
        } else if(intent.HasExtra("google.message_id")) {
            return new FCMNotification(
                intent.Extras.GetString(BundleKeyBody),
                intent.Extras.GetString(BundleKeyTitle),
                intent.Extras.GetString(BundleKeyImageUrl),
                intent.Extras.ToDictionary());
        } else {
            throw new FCMException("Couldn't get notification from intent extras");
        }
    }

    public static bool IsNotificationTappedIntent(this Intent intent, string extraName)
    {
        try {
            return intent.GetNotificationFromExtras(extraName) != null;
        } catch(Exception) {
            return false;
        }
    }
}