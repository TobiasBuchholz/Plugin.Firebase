using System.Collections.Generic;
using System.Linq;
using Foundation;
using Plugin.Firebase.Abstractions.CloudMessaging;
using UserNotifications;

namespace Plugin.Firebase.iOS.Extensions
{
    public static class FCMNotificationExtension
    {
        public static FCMNotification ToFCMNotification(this UNNotification notification)
        {
            return notification.Request.Content.UserInfo.ToFCMNotification();
        }
        
        public static FCMNotification ToFCMNotification(this NSDictionary userInfo)
        {
            var apsDict = userInfo.ValueForKey(new NSString("aps"));
            if(apsDict == null) {
                var notification = userInfo.ValueForKey(new NSString("notification"));
                if(notification is NSDictionary dict) {
                    return new FCMNotification(dict.ValueForKey(new NSString("body"))?.ToString(), dict.ValueForKey(new NSString("title"))?.ToString(), ExctractData(userInfo));
                }
            } else {
                var alert = apsDict.ValueForKey(new NSString("alert"));
                if(alert is NSDictionary dict) {
                    return new FCMNotification(dict.ValueForKey(new NSString("body"))?.ToString(), dict.ValueForKey(new NSString("title"))?.ToString(), ExctractData(userInfo));
                } else if(alert != null) {
                    return new FCMNotification(alert.ToString(), "", ExctractData(userInfo));
                } 
            }
            return FCMNotification.Empty();
        }

        private static Dictionary<string, string> ExctractData(NSDictionary userInfo)
        {
            return userInfo
                .Select(x => x)
                .ToList()
                .ToDictionary(x => x.Key.ToString(), x => x.Value.ToString());
        }
    }
}