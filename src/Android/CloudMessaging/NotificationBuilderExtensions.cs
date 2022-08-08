using System;
using Android.Graphics;
using AndroidX.Core.App;
using Java.Net;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Common;

namespace Plugin.Firebase.Android.CloudMessaging
{
    public static class NotificationBuilderExtensions
    {
        public static NotificationCompat.Builder TrySetBigPictureStyle(this NotificationCompat.Builder @this, FCMNotification notification)
        {
            try {
                var bitmap = DecodeBitmap(notification.ImageUrl);
                return @this.SetLargeIcon(bitmap).SetStyle(new NotificationCompat.BigPictureStyle().BigPicture(bitmap).BigLargeIcon(null));
            } catch(Exception e) {
                Console.WriteLine($"[Plugin.Firebase]: Couldn't attach image to push notification: {e.Message}");
                return @this;
            }
        }

        private static Bitmap DecodeBitmap(string url)
        {
            var connection = (URLConnection) new URL(url).OpenConnection();
            return connection == null
                ? throw new FirebaseException($"Couldn't open connection to url: {url}")
                : DecodeBitmap(connection);
        }

        private static Bitmap DecodeBitmap(URLConnection connection)
        {
            connection.DoInput = true;
            connection.Connect();
            var bitmap = BitmapFactory.DecodeStream(connection.InputStream);
            connection.Dispose();
            return bitmap;
        }
    }
}