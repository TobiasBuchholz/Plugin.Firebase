using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;

namespace Plugin.Firebase.IntegrationTests.CloudMessaging
{
    [Collection("Sequential")]
    [TestLogging]
    [IntegrationTestFixture(IntegrationTestPackage.CloudMessaging)]
    [Preserve(AllMembers = true)]
    public sealed class CloudMessagingFixture
    {
        [Fact]
        public async Task raises_notification_received_for_synthetic_notification()
        {
            var sut = CrossFirebaseCloudMessaging.Current;
            using var notificationReceived = new EventProbe<FCMNotificationReceivedEventArgs>(
                handler => sut.NotificationReceived += handler,
                handler => sut.NotificationReceived -= handler);

            sut.OnNotificationReceived(new FCMNotification(
                data: new Dictionary<string, string> {
                    { "title", "Synthetic title" },
                    { "body", "Synthetic body" },
                    { "is_silent_in_foreground", "true" },
                    { "custom", "value" }
                }));

            var args = await notificationReceived.WaitAsync(
                IntegrationTestTimeouts.ShortCallback,
                "synthetic notification delivery");

            Assert.Equal("Synthetic title", args.Notification.Title);
            Assert.Equal("Synthetic body", args.Notification.Body);
            Assert.True(args.Notification.IsSilentInForeground);
            Assert.Equal("value", args.Notification.Data["custom"]);
        }

        [Fact]
        public async Task replays_notification_tap_when_handler_is_registered_late()
        {
            var sut = CrossFirebaseCloudMessaging.Current;
            var missedNotificationField = sut.GetType().GetField(
                "_missedTappedNotification",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(missedNotificationField);

            var expectedNotification = new FCMNotification(
                body: "Tapped body",
                title: "Tapped title",
                data: new Dictionary<string, string> { { "source", "reflection" } });
            missedNotificationField.SetValue(sut, expectedNotification);

            using var notificationTapped = new EventProbe<FCMNotificationTappedEventArgs>(
                handler => sut.NotificationTapped += handler,
                handler => sut.NotificationTapped -= handler);

            var args = await notificationTapped.WaitAsync(
                IntegrationTestTimeouts.ShortCallback,
                "late notification tap replay");

            Assert.Equal("Tapped title", args.Notification.Title);
            Assert.Equal("Tapped body", args.Notification.Body);
            Assert.Equal("reflection", args.Notification.Data["source"]);
        }

        [RealFirebaseOptInFact(IntegrationTestOptions.RunFcmTokenTestsEnvironmentVariableName, skipIosSimulator: true)]
        public async Task gets_token_raises_token_changed_and_manages_topic_when_enabled()
        {
            var sut = CrossFirebaseCloudMessaging.Current;
            using var tokenChanged = new EventProbe<FCMTokenChangedEventArgs>(
                handler => sut.TokenChanged += handler,
                handler => sut.TokenChanged -= handler);
            var topic = IntegrationTestData.UniqueId("acceptance");

            var token = await sut.GetTokenAsync();
            await sut.OnTokenRefreshAsync();
            var args = await tokenChanged.WaitAsync(
                IntegrationTestTimeouts.LongCallback,
                "FCM token refresh event");
            await sut.CheckIfValidAsync();
            await sut.SubscribeToTopicAsync(topic);
            await sut.UnsubscribeFromTopicAsync(topic);

            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.False(string.IsNullOrWhiteSpace(args.Token));
        }

        [RealFirebaseOptInFact(IntegrationTestOptions.RunFcmDeliveryTestsEnvironmentVariableName, skipIosSimulator: true)]
        public async Task receives_real_push_delivery_when_enabled()
        {
            var sut = CrossFirebaseCloudMessaging.Current;
            var token = await sut.GetTokenAsync();
            using var notificationReceived = new EventProbe<FCMNotificationReceivedEventArgs>(
                handler => sut.NotificationReceived += handler,
                handler => sut.NotificationReceived -= handler);

            TestLog.Write($"[FCM TOKEN] {token}");
            var args = await notificationReceived.WaitAsync(
                IntegrationTestTimeouts.FcmDelivery,
                "real FCM push delivery");
            Assert.NotNull(args.Notification);
        }
    }
}