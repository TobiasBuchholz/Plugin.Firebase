using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;
using Plugin.Firebase.CloudMessaging.Platforms.iOS.Extensions;
using UserNotifications;
using NSDictionary = Foundation.NSDictionary;
using NSNumber = Foundation.NSNumber;

namespace Plugin.Firebase.IntegrationTests.CloudMessaging;

[Collection("Sequential")]
[TestLogging]
[IntegrationTestFixture(IntegrationTestPackage.CloudMessaging)]
[Preserve(AllMembers = true)]
public sealed class CloudMessagingIosFixture
{
    private const string SilentInForegroundKey = "is_silent_in_foreground";
    private const string ActionIdentifier = "plugin-firebase-test-action";
    private const string ActionIdentifierKey = "ActionIdentifier";
    // value of UNNotificationDefaultActionIdentifier, whose binding is not public
    private const string DefaultActionIdentifier = "com.apple.UNNotificationDefaultActionIdentifier";
    private const UNNotificationPresentationOptions VisibleOptions =
        UNNotificationPresentationOptions.Banner
        | UNNotificationPresentationOptions.List
        | UNNotificationPresentationOptions.Sound;

    private static FirebaseCloudMessagingImplementation Sut =>
        (FirebaseCloudMessagingImplementation) CrossFirebaseCloudMessaging.Current;

    [IosFact]
    public async Task completes_silent_foreground_notification_once_with_no_presentation_options()
    {
        var notification = CreateNotification(isSilentInForeground: true);
        using var received = ProbeNotificationReceived();
        var completions = new List<UNNotificationPresentationOptions>();

        Sut.CompleteForegroundNotification(() => notification, completions.Add);

        var args = await received.WaitAsync(IntegrationTestTimeouts.ShortCallback, "silent foreground notification");
        Assert.Same(notification, args.Notification);
        Assert.Equal(UNNotificationPresentationOptions.None, Assert.Single(completions));
    }

    [IosFact]
    public async Task completes_visible_foreground_notification_once_with_banner_list_and_sound()
    {
        var notification = CreateNotification(isSilentInForeground: false);
        using var received = ProbeNotificationReceived();
        var completions = new List<UNNotificationPresentationOptions>();

        Sut.CompleteForegroundNotification(() => notification, completions.Add);

        var args = await received.WaitAsync(IntegrationTestTimeouts.ShortCallback, "visible foreground notification");
        Assert.Same(notification, args.Notification);
        Assert.Equal(VisibleOptions, Assert.Single(completions));
    }

    [IosFact]
    public async Task completes_with_no_presentation_options_when_subscriber_marks_notification_silent()
    {
        var notification = CreateNotification(isSilentInForeground: false);
        using var received = ProbeNotificationReceived(
            args => args.Notification.Data[SilentInForegroundKey] = "true");
        var completions = new List<UNNotificationPresentationOptions>();

        Sut.CompleteForegroundNotification(() => notification, completions.Add);

        await received.WaitAsync(IntegrationTestTimeouts.ShortCallback, "foreground notification marked silent");
        Assert.Equal(UNNotificationPresentationOptions.None, Assert.Single(completions));
    }

    [IosFact]
    public async Task completes_foreground_notification_once_when_subscriber_throws()
    {
        var notification = CreateNotification(isSilentInForeground: false);
        using var received = ProbeNotificationReceived(
            _ => throw new InvalidOperationException("subscriber failure"));
        var completions = new List<UNNotificationPresentationOptions>();

        var exception = Assert.Throws<InvalidOperationException>(
            () => Sut.CompleteForegroundNotification(() => notification, completions.Add));

        await received.WaitAsync(IntegrationTestTimeouts.ShortCallback, "foreground notification with throwing subscriber");
        Assert.Equal("subscriber failure", exception.Message);
        Assert.Equal(VisibleOptions, Assert.Single(completions));
    }

    [IosFact]
    public void completes_foreground_notification_once_when_conversion_throws()
    {
        var completions = new List<UNNotificationPresentationOptions>();

        var exception = Assert.Throws<InvalidOperationException>(
            () => Sut.CompleteForegroundNotification(
                () => throw new InvalidOperationException("conversion failure"),
                completions.Add));

        Assert.Equal("conversion failure", exception.Message);
        Assert.Equal(VisibleOptions, Assert.Single(completions));
    }

    [IosFact]
    public async Task completes_notification_response_once_when_tapped_subscriber_throws()
    {
        var notification = CreateNotification(isSilentInForeground: false);
        using var tapped = ProbeNotificationTapped(
            _ => throw new InvalidOperationException("subscriber failure"));
        var completions = 0;

        var exception = Assert.Throws<InvalidOperationException>(
            () => Sut.CompleteNotificationResponse(() => notification, () => ActionIdentifier, () => completions++));

        var args = await tapped.WaitAsync(IntegrationTestTimeouts.ShortCallback, "notification tap with throwing subscriber");
        Assert.Same(notification, args.Notification);
        Assert.Equal(ActionIdentifier, args.Notification.Data[ActionIdentifierKey]);
        Assert.Equal("subscriber failure", exception.Message);
        Assert.Equal(1, completions);
    }

    [IosFact]
    public async Task completes_notification_response_once_when_tap_is_kept_for_late_subscriber()
    {
        var notification = CreateNotification(isSilentInForeground: false);
        var completions = 0;

        Sut.CompleteNotificationResponse(() => notification, () => ActionIdentifier, () => completions++);
        using var tapped = ProbeNotificationTapped();

        var args = await tapped.WaitAsync(IntegrationTestTimeouts.ShortCallback, "kept notification tap replay");
        Assert.Same(notification, args.Notification);
        Assert.Equal(1, completions);
    }

    [IosFact]
    public async Task replays_kept_tap_with_custom_action_identifier_replacing_sender_value()
    {
        var notification = CreateNotification(isSilentInForeground: false);
        notification.Data[ActionIdentifierKey] = "sender-value";

        Sut.CompleteNotificationResponse(() => notification, () => ActionIdentifier, () => { });
        using var tapped = ProbeNotificationTapped();

        var args = await tapped.WaitAsync(IntegrationTestTimeouts.ShortCallback, "kept tap with custom action");
        Assert.Equal(ActionIdentifier, args.Notification.Data[ActionIdentifierKey]);
        Assert.Equal("value", args.Notification.Data["custom"]);
    }

    [IosFact]
    public async Task raises_tap_for_subscriber_with_default_action_identifier()
    {
        var notification = CreateNotification(isSilentInForeground: false);
        using var tapped = ProbeNotificationTapped();

        Sut.CompleteNotificationResponse(() => notification, () => DefaultActionIdentifier, () => { });

        var args = await tapped.WaitAsync(IntegrationTestTimeouts.ShortCallback, "tap with default action");
        Assert.Equal(DefaultActionIdentifier, args.Notification.Data[ActionIdentifierKey]);
        Assert.Equal("value", args.Notification.Data["custom"]);
    }

    [IosFact]
    public async Task raises_tap_on_local_notification_with_user_info_and_action_identifier()
    {
        var request = CreateLocalNotificationRequest();
        using var tapped = ProbeNotificationTapped();

        Sut.CompleteNotificationResponse(() => request.ToFCMNotification(), () => DefaultActionIdentifier, () => { });

        var args = await tapped.WaitAsync(IntegrationTestTimeouts.ShortCallback, "local notification tap");
        Assert.Equal("orders/42", args.Notification.Data["route"]);
        Assert.Equal(DefaultActionIdentifier, args.Notification.Data[ActionIdentifierKey]);
        Assert.Equal(("Local body", "Local title"), (args.Notification.Body, args.Notification.Title));
    }

    [IosFact]
    public async Task completes_local_notification_marked_silent_by_subscriber_with_no_presentation_options()
    {
        var request = CreateLocalNotificationRequest();
        using var received = ProbeNotificationReceived(
            args => args.Notification.Data[SilentInForegroundKey] = "true");
        var completions = new List<UNNotificationPresentationOptions>();

        Sut.CompleteForegroundNotification(() => request.ToFCMNotification(), completions.Add);

        var args = await received.WaitAsync(IntegrationTestTimeouts.ShortCallback, "local notification marked silent");
        Assert.Equal("orders/42", args.Notification.Data["route"]);
        Assert.Equal(UNNotificationPresentationOptions.None, Assert.Single(completions));
    }

    [IosFact]
    public void converts_local_notification_user_info_into_data()
    {
        var userInfo = new NSDictionary("route", "orders/42");

        var notification = userInfo.ToFCMNotification();

        Assert.Equal("orders/42", notification.Data["route"]);
        Assert.Equal(("", ""), (notification.Body, notification.Title));
    }

    [IosFact]
    public void converts_aps_payload_without_alert_keeping_user_info_in_data()
    {
        var userInfo = new NSDictionary(
            "aps", new NSDictionary("content-available", NSNumber.FromInt32(1)),
            "fcm_options", new NSDictionary("image", "https://example.com/image.png"),
            "title", "Data title",
            "route", "orders/42");

        var notification = userInfo.ToFCMNotification();

        Assert.Equal("orders/42", notification.Data["route"]);
        Assert.Equal(("", "Data title"), (notification.Body, notification.Title));
        Assert.Equal("https://example.com/image.png", notification.ImageUrl);
    }

    [IosFact]
    public void converts_local_notification_request_with_displayed_title_and_body()
    {
        var request = CreateLocalNotificationRequest();

        var notification = request.ToFCMNotification();

        Assert.Equal(("Local body", "Local title"), (notification.Body, notification.Title));
        Assert.Equal("orders/42", notification.Data["route"]);
    }

    [IosFact]
    public void converts_local_notification_request_preferring_user_info_title_and_body()
    {
        var request = CreateLocalNotificationRequest(
            new NSDictionary("route", "orders/42", "title", "Data title", "body", "Data body"));

        var notification = request.ToFCMNotification();

        Assert.Equal(("Data body", "Data title"), (notification.Body, notification.Title));
    }

    [IosFact]
    public void converts_local_notification_request_filling_only_missing_title_or_body()
    {
        var request = CreateLocalNotificationRequest(new NSDictionary("title", "Data title"));

        var notification = request.ToFCMNotification();

        Assert.Equal(("Local body", "Data title"), (notification.Body, notification.Title));
    }

    [IosFact]
    public void converts_request_with_aps_alert_from_user_info_only()
    {
        var request = CreateLocalNotificationRequest(new NSDictionary(
            "aps", new NSDictionary("alert", new NSDictionary("body", "Alert body")),
            "route", "orders/42"));

        var notification = request.ToFCMNotification();

        Assert.Equal(("Alert body", ""), (notification.Body, notification.Title));
        Assert.Equal("orders/42", notification.Data["route"]);
    }

    private static UNNotificationRequest CreateLocalNotificationRequest(NSDictionary? userInfo = null)
    {
        var content = new UNMutableNotificationContent {
            Title = "Local title",
            Body = "Local body",
            UserInfo = userInfo ?? new NSDictionary("route", "orders/42")
        };
        return UNNotificationRequest.FromIdentifier("plugin-firebase-local-notification", content, null);
    }

    private static FCMNotification CreateNotification(bool isSilentInForeground)
    {
        var data = new Dictionary<string, string> { { "custom", "value" } };
        if(isSilentInForeground) {
            data[SilentInForegroundKey] = "true";
        }

        return new FCMNotification(body: "Delegate body", title: "Delegate title", data: data);
    }

    private static EventProbe<FCMNotificationReceivedEventArgs> ProbeNotificationReceived(
        Action<FCMNotificationReceivedEventArgs>? subscriber = null)
    {
        var sut = Sut;
        return ProbeThenSubscribe<FCMNotificationReceivedEventArgs>(
            handler => sut.NotificationReceived += handler,
            handler => sut.NotificationReceived -= handler,
            subscriber);
    }

    private static EventProbe<FCMNotificationTappedEventArgs> ProbeNotificationTapped(
        Action<FCMNotificationTappedEventArgs>? subscriber = null)
    {
        var sut = Sut;
        return ProbeThenSubscribe<FCMNotificationTappedEventArgs>(
            handler => sut.NotificationTapped += handler,
            handler => sut.NotificationTapped -= handler,
            subscriber);
    }

    private static EventProbe<TEventArgs> ProbeThenSubscribe<TEventArgs>(
        Action<EventHandler<TEventArgs>> subscribe,
        Action<EventHandler<TEventArgs>> unsubscribe,
        Action<TEventArgs>? subscriber)
    {
        // the probe subscribes first, so it records the event even when the app subscriber throws
        EventHandler<TEventArgs> appSubscriber = (_, args) => subscriber?.Invoke(args);
        return new EventProbe<TEventArgs>(
            handler => {
                subscribe(handler);
                subscribe(appSubscriber);
            },
            handler => {
                unsubscribe(appSubscriber);
                unsubscribe(handler);
            });
    }
}