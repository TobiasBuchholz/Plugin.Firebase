using Plugin.Firebase.CloudMessaging;

namespace Plugin.Firebase.IntegrationTests.CloudMessaging;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestFixture(IntegrationTestPackage.CloudMessaging)]
[Preserve(AllMembers = true)]
public sealed class CloudMessagingAndroidFixture
{
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
}