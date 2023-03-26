using Foundation;
using UserNotifications;

namespace Plugin.Firebase.CloudMessaging.Platforms.iOS.Extensions;

public static class FCMNotificationExtension
{
    public static FCMNotification ToFCMNotification(this UNNotification notification)
    {
        return notification.Request.ToFCMNotification();
    }

    public static FCMNotification ToFCMNotification(this UNNotificationRequest request)
    {
        return request.Content.UserInfo.ToFCMNotification();
    }

    public static FCMNotification ToFCMNotification(this NSDictionary userInfo)
    {
        if(userInfo["aps"] is NSDictionary apsDict) {
            var alert = apsDict["alert"];
            if(alert is NSDictionary dict) {
                return new FCMNotification(GetBody(dict), GetTitle(dict), GetImageUrl(userInfo), userInfo.ToDictionary());
            } else if(alert != null) {
                return new FCMNotification(alert.ToString(), "", "", userInfo.ToDictionary());
            }
        } else {
            var notification = userInfo["notification"];
            if(notification is NSDictionary dict) {
                return new FCMNotification(GetBody(dict), GetTitle(dict), GetImageUrl(dict), userInfo.ToDictionary());
            }
        }
        return FCMNotification.Empty();
    }

    private static string GetBody(NSDictionary dict)
    {
        return dict["body"]?.ToString();
    }

    private static string GetTitle(NSDictionary dict)
    {
        return dict["title"]?.ToString();
    }

    private static string GetImageUrl(NSDictionary dict)
    {
        var fcmOptions = dict["fcm_options"] as NSDictionary;
        return fcmOptions?["image"]?.ToString();
    }

    private static Dictionary<string, string> ToDictionary(this NSDictionary dictionary)
    {
        return dictionary.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
    }
}