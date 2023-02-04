using Firebase.CloudFirestore;

namespace Plugin.Firebase.Firestore.iOS.Extensions;

public static class DateExtensions
{
    public static DateTimeOffset ToDateTimeOffset(this Timestamp @this)
    {
        return @this == null ? default(DateTimeOffset) : DateTime.SpecifyKind((DateTime) @this.DateValue, DateTimeKind.Utc);
    }
}