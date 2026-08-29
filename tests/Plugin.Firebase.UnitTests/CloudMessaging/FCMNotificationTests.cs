using Plugin.Firebase.CloudMessaging;

namespace Plugin.Firebase.UnitTests.CloudMessaging;

public class FCMNotificationTests
{
    [Fact]
    public void preserves_legacy_constructor_signature()
    {
        var constructor = typeof(FCMNotification).GetConstructor([
            typeof(string),
            typeof(string),
            typeof(string),
            typeof(IDictionary<string, string>)
        ]);

        Assert.NotNull(constructor);
    }

    [Fact]
    public void channel_id_defaults_to_null()
    {
        var notification = new FCMNotification();

        Assert.Null(notification.ChannelId);
    }

    [Fact]
    public void preserves_channel_id_from_initializer()
    {
        const string channelId = "alerts";
        var notification = new FCMNotification {
            ChannelId = channelId
        };

        Assert.Equal(channelId, notification.ChannelId);
    }
}