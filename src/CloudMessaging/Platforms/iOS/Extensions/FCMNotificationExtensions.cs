using UserNotifications;

namespace Plugin.Firebase.CloudMessaging.Platforms.iOS.Extensions;

/// <summary>
/// Provides extension methods for converting iOS notification types to <see cref="FCMNotification"/>.
/// </summary>
public static class FCMNotificationExtension
{
    /// <summary>
    /// Converts a <see cref="UNNotification"/> to an <see cref="FCMNotification"/>.
    /// </summary>
    /// <param name="notification">The iOS notification to convert.</param>
    /// <returns>An <see cref="FCMNotification"/> containing the notification data.</returns>
    public static FCMNotification ToFCMNotification(this UNNotification notification)
    {
        return notification.Request.ToFCMNotification();
    }

    /// <summary>
    /// Converts a <see cref="UNNotificationRequest"/> to an <see cref="FCMNotification"/>.
    /// </summary>
    /// <param name="request">The iOS notification request to convert.</param>
    /// <returns>An <see cref="FCMNotification"/> containing the notification data.</returns>
    /// <remarks>
    /// The request's <see cref="UNNotificationContent.UserInfo"/> is converted as described for
    /// <see cref="ToFCMNotification(NSDictionary)"/>. When the user info has no alert or <c>notification</c>
    /// dictionary (for example a local notification) and gives no <see cref="FCMNotification.Body"/> or
    /// <see cref="FCMNotification.Title"/>, the displayed <see cref="UNNotificationContent.Body"/> and
    /// <see cref="UNNotificationContent.Title"/> are used instead. A payload with an <c>aps.alert</c> or
    /// <c>notification</c> dictionary is converted from the user info only.
    /// </remarks>
    public static FCMNotification ToFCMNotification(this UNNotificationRequest request)
    {
        var content = request.Content;
        var notification = content.UserInfo.ToFCMNotification();
        if(HasAlert(content.UserInfo)) {
            return notification;
        }

        var body = FallbackOrNull(notification.Body, content.Body);
        var title = FallbackOrNull(notification.Title, content.Title);
        if(body == null && title == null) {
            return notification;
        }
        return new FCMNotification(body, title, notification.ImageUrl, notification.Data);
    }

    private static bool HasAlert(NSDictionary userInfo)
    {
        return userInfo["aps"] is NSDictionary apsDict
            ? apsDict["alert"] != null
            : userInfo["notification"] is NSDictionary;
    }

    // null keeps the converted value, which may still come from the body or title data key
    private static string FallbackOrNull(string converted, string displayed)
    {
        return string.IsNullOrEmpty(converted) && !string.IsNullOrEmpty(displayed) ? displayed : null;
    }

    /// <summary>
    /// Converts an <see cref="NSDictionary"/> containing notification user info to an <see cref="FCMNotification"/>.
    /// </summary>
    /// <param name="userInfo">The user info dictionary from the notification.</param>
    /// <returns>
    /// An <see cref="FCMNotification"/> containing the notification data. When the payload has no alert or
    /// <c>notification</c> dictionary (for example a local notification), <see cref="FCMNotification.Data"/>
    /// holds the user info entries, and <see cref="FCMNotification.Body"/> and <see cref="FCMNotification.Title"/>
    /// come from its <c>body</c> and <c>title</c> keys when present. The
    /// <see cref="ToFCMNotification(UNNotificationRequest)"/> overload also falls back to the displayed title and body.
    /// </returns>
    public static FCMNotification ToFCMNotification(this NSDictionary userInfo)
    {
        if(userInfo["aps"] is NSDictionary apsDict) {
            var alert = apsDict["alert"];
            if(alert is NSDictionary dict) {
                return new FCMNotification(
                    GetBody(dict),
                    GetTitle(dict),
                    GetImageUrl(userInfo),
                    userInfo.ToDictionary()
                );
            } else if(alert != null) {
                return new FCMNotification(alert.ToString(), "", "", userInfo.ToDictionary());
            }
        } else {
            var notification = userInfo["notification"];
            if(notification is NSDictionary dict) {
                return new FCMNotification(
                    GetBody(dict),
                    GetTitle(dict),
                    GetImageUrl(dict),
                    userInfo.ToDictionary()
                );
            }
        }
        // no alert or notification dictionary: keep the user info so custom keys aren't dropped
        return new FCMNotification(imageUrl: GetImageUrl(userInfo), data: userInfo.ToDictionary());
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