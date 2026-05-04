namespace Plugin.Firebase.IntegrationTests;

internal static class IntegrationTestTimeouts
{
    public static readonly TimeSpan ShortCallback = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan Callback = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan LongCallback = TimeSpan.FromSeconds(20);
    public static readonly TimeSpan Cleanup = TimeSpan.FromSeconds(15);
    public static readonly TimeSpan FcmDelivery = TimeSpan.FromMinutes(2);
    public static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    public static long OneMillisecondTicks => TimeSpan.FromMilliseconds(1).Ticks;
}