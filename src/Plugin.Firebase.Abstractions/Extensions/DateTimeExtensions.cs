namespace System.Collections.Generic
{
    public static class DateTimeExtensions
    {
        public static DateTime TruncateToSeconds(this DateTime dateTime)
        {
            return dateTime.Truncate(TimeSpan.FromSeconds(1));
        }
        
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
        {
            if(timeSpan == TimeSpan.Zero || dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) {
                return dateTime; 
            } else {
                return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
            }
        }
    }
}