using System;
using Firebase.CloudFirestore;
using Foundation;

namespace Plugin.Firebase.iOS.Extensions
{
    public static class DateExtensions
    {
        public static NSDate ToNSDate(this DateTime @this)
        {
            if(@this.Kind == DateTimeKind.Unspecified) {
                @this = DateTime.SpecifyKind(@this, DateTimeKind.Local);
            }
            return (NSDate) @this;
        }

        public static DateTime ToDateTime(this NSDate @this)
        {
            return ((DateTime) @this);
        }

        public static DateTime ToDateTime(this Timestamp @this)
        {
            return @this.DateValue.ToDateTime();
        }
    }
}