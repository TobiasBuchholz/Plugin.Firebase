using System.Collections.Generic;
using System.Linq;
using Foundation;
using Plugin.Firebase.CloudMessaging;
using UserNotifications;

namespace Plugin.Firebase.iOS.CloudMessaging
{
    public static class FCMNotificationExtension
    {
        public static FCMNotification ToFCMNotification(this UNNotification notification)
        {
            return notification.Request.Content.UserInfo.ToFCMNotification();
        }
        
        public static FCMNotification ToFCMNotification(this NSDictionary userInfo)
        {
            if(userInfo["aps"] is NSDictionary apsDict) {
                var alert = apsDict["alert"];
                if(alert is NSDictionary dict) {
                    return new FCMNotification(dict["body"]?.ToString(), dict["title"]?.ToString(), userInfo.ToDictionary());
                } else if(alert != null) {
                    return new FCMNotification(alert.ToString(), "", userInfo.ToDictionary());
                } 
            } else {
                var notification = userInfo["notification"];
                if(notification is NSDictionary dict) {
                    return new FCMNotification(dict["body"]?.ToString(), dict["title"]?.ToString(), userInfo.ToDictionary());
                }
            }
            return FCMNotification.Empty();
        }

        private static Dictionary<string, string> ToDictionary(this NSDictionary dictionary)
        {
            return dictionary.ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
        }
    }
}