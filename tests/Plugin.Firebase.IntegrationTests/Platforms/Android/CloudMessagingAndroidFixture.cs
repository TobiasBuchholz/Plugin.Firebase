using Android.OS;
using Firebase.Messaging;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.Platforms.Android.Extensions;

namespace Plugin.Firebase.IntegrationTests.CloudMessaging;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestFixture(IntegrationTestPackage.CloudMessaging)]
[Preserve(AllMembers = true)]
public sealed class CloudMessagingAndroidFixture
{
    private const string NotificationEnabledKey = "gcm.n.e";
    private const string NotificationChannelIdKey = "gcm.n.android_channel_id";

    [Fact]
    public void maps_android_channel_id_from_remote_message()
    {
        const string expectedChannelId = "plugin-firebase-test-alerts";
        using var payload = CreateRemoteNotificationPayload(expectedChannelId);
        using var remoteMessage = new RemoteMessage(payload);

        var nativeNotification = remoteMessage.GetNotification();
        var notification = remoteMessage.ToFCMNotification();

        Assert.NotNull(nativeNotification);
        Assert.Equal(expectedChannelId, nativeNotification?.ChannelId);
        Assert.Equal(expectedChannelId, notification.ChannelId);
        Assert.Equal("Channel title", notification.Title);
        Assert.Equal("Channel body", notification.Body);
        Assert.Equal("value", notification.Data["custom"]);
        Assert.DoesNotContain(NotificationChannelIdKey, notification.Data.Keys);
    }

    [Fact]
    public void forwards_android_channel_id_to_foreground_notification_action()
    {
        const string expectedChannelId = "plugin-firebase-test-alerts";
        using var payload = CreateRemoteNotificationPayload(expectedChannelId);
        using var remoteMessage = new RemoteMessage(payload);
        var notification = remoteMessage.ToFCMNotification();
        FCMNotification? capturedNotification = null;
        FirebaseCloudMessagingImplementation.ShowLocalNotificationAction = value => capturedNotification = value;

        try {
            CrossFirebaseCloudMessaging.Current.OnNotificationReceived(notification);

            Assert.Same(notification, capturedNotification);
            Assert.Equal(expectedChannelId, capturedNotification?.ChannelId);
        }
        finally {
            FirebaseCloudMessagingImplementation.ShowLocalNotificationAction = null;
        }
    }

    [Fact]
    public void preserves_android_channel_id_through_notification_bundle_round_trip()
    {
        const string expectedChannelId = "plugin-firebase-test-alerts";
        using var payload = CreateRemoteNotificationPayload(expectedChannelId);
        using var remoteMessage = new RemoteMessage(payload);
        var notification = remoteMessage.ToFCMNotification();

        using var bundle = notification.ToBundle();
        var roundTrippedNotification = bundle.ToFCMNotification();

        Assert.Equal(expectedChannelId, roundTrippedNotification.ChannelId);
        Assert.Equal("Channel title", roundTrippedNotification.Title);
        Assert.Equal("Channel body", roundTrippedNotification.Body);
        Assert.Equal("value", roundTrippedNotification.Data["custom"]);
    }

    [Fact]
    public void leaves_android_channel_id_null_when_remote_message_omits_it()
    {
        using var payload = CreateRemoteNotificationPayload(channelId: null);
        using var remoteMessage = new RemoteMessage(payload);

        var nativeNotification = remoteMessage.GetNotification();
        var notification = remoteMessage.ToFCMNotification();

        Assert.NotNull(nativeNotification);
        Assert.Null(nativeNotification?.ChannelId);
        Assert.Null(notification.ChannelId);
    }

    [Fact]
    public void silent_foreground_notification_suppresses_android_local_notification_action()
    {
        var wasInvoked = false;
        FirebaseCloudMessagingImplementation.ShowLocalNotificationAction = _ => wasInvoked = true;

        try {
            CrossFirebaseCloudMessaging.Current.OnNotificationReceived(
                new FCMNotification(
                    data: new Dictionary<string, string> {
                        { "title", "Silent" },
                        { "body", "No local notification" },
                        { "is_silent_in_foreground", "true" }
                    }));

            Assert.False(wasInvoked);
        }
        finally {
            FirebaseCloudMessagingImplementation.ShowLocalNotificationAction = null;
        }
    }

    private static Bundle CreateRemoteNotificationPayload(string? channelId)
    {
        var payload = new Bundle();
        payload.PutString(NotificationEnabledKey, "1");
        payload.PutString("gcm.n.title", "Channel title");
        payload.PutString("gcm.n.body", "Channel body");
        payload.PutString("custom", "value");

        if(channelId != null) {
            payload.PutString(NotificationChannelIdKey, channelId);
        }

        return payload;
    }
}
