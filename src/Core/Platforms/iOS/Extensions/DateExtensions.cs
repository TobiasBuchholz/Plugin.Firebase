using Foundation;

namespace Plugin.Firebase.Core.Platforms.iOS.Extensions;

public static class DateExtensions
{
    public static NSDate ToNSDate(this DateTime @this)
    {
        if(@this.Kind == DateTimeKind.Unspecified) {
            @this = DateTime.SpecifyKind(@this, DateTimeKind.Utc);
        }
        return (NSDate) @this;
    }
    
    public static NSDate ToNSDate(this DateTimeOffset @this)
    {
        return @this.DateTime.ToNSDate();
    }

    public static DateTimeOffset ToDateTimeOffset(this NSDate @this)
    {
        return @this == null ? default(DateTimeOffset) : DateTime.SpecifyKind((DateTime) @this, DateTimeKind.Utc);
    }
}