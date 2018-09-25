using System;
using Java.Util;

namespace Plugin.Firebase.Android.Extensions
{
    public static class DateExtensions
    {
        public static DateTime ToDateTime(this Date date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddMilliseconds(date.Time);
        }

        public static Date ToJavaDate(this DateTime @this)
        {
            var millis = (long) @this.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            return new Date(millis);
        }
    }
}