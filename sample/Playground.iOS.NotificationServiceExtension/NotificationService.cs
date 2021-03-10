using System;
using System.IO;
using System.Net;
using Foundation;
using UIKit;
using UserNotifications;

namespace Playground.iOS.NotificationServiceExtension
{
    [Register("NotificationService")]
    public class NotificationService : UNNotificationServiceExtension
    {
        protected NotificationService(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void DidReceiveNotificationRequest(UNNotificationRequest request, Action<UNNotificationContent> contentHandler)
        {
            var content = (UNMutableNotificationContent) request.Content.MutableCopy();
            TryAttachImage(content, request);
            contentHandler(content);
        }
        
        private static void TryAttachImage(UNMutableNotificationContent content, UNNotificationRequest request)
        {
            try {
                content.Attachments = new[] { CreateImageAttachment(request) };
            } catch(Exception e) {
                Console.Write($"Couldn't attach image to push notification: {e.Message}");
            }
        }

        private static UNNotificationAttachment CreateImageAttachment(UNNotificationRequest request)
        {
            var filePath = DownloadAttachedImageFile(request);
            var options = new UNNotificationAttachmentOptions();
            var fileUrl = NSUrl.CreateFileUrl(filePath, false, null);
            var attachment = UNNotificationAttachment.FromIdentifier("image", fileUrl, options, out var error);
            return error == null ? attachment : throw new Exception(error.LocalizedDescription);
        }

        private static string DownloadAttachedImageFile(UNNotificationRequest request)
        {
            var fcmOptions = (NSDictionary) request.Content.UserInfo.ObjectForKey(new NSString("fcm_options"));
            var imageUrl = (NSString) fcmOptions.ObjectForKey(new NSString("image"));
            return DownloadFile(imageUrl, $"push_image_{DateTime.Now.Ticks}.jpg");
        }

        private static string DownloadFile(string url, string fileName)
        {
            var webClient = new WebClient();
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            webClient.DownloadFile(new Uri(url), filePath);
            return filePath;
        }

        public override void TimeWillExpire()
        {
            // Called just before the extension will be terminated by the system.
            // Use this as an opportunity to deliver your "best attempt" at modified content, otherwise the original push payload will be used.
        }
    }
}
