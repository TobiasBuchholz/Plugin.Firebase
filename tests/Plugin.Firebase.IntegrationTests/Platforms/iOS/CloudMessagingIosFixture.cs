using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;
using UserNotifications;

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
        Assert.Equal(ActionIdentifier, args.Notification.Data["ActionIdentifier"]);
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
    public async Task raises_tap_without_data_as_copy_holding_only_action_identifier()
    {
        var notification = new FCMNotification(body: "Delegate body", title: "Delegate title");
        using var tapped = ProbeNotificationTapped();

        Sut.CompleteNotificationResponse(() => notification, () => DefaultActionIdentifier, () => { });

        var args = await tapped.WaitAsync(IntegrationTestTimeouts.ShortCallback, "tap without data");
        Assert.Equal(KeyValuePair.Create(ActionIdentifierKey, DefaultActionIdentifier), Assert.Single(args.Notification.Data));
        Assert.Equal(("Delegate body", "Delegate title"), (args.Notification.Body, args.Notification.Title));
        Assert.Null(notification.Data);
    }

    [IosFact]
    public async Task replays_kept_tap_without_data_as_copy_holding_only_action_identifier()
    {
        // userInfo with neither aps.alert nor a notification dictionary converts to FCMNotification.Empty()
        Sut.CompleteNotificationResponse(() => FCMNotification.Empty(), () => ActionIdentifier, () => { });
        using var tapped = ProbeNotificationTapped();

        var args = await tapped.WaitAsync(IntegrationTestTimeouts.ShortCallback, "kept tap without data");
        Assert.Equal(KeyValuePair.Create(ActionIdentifierKey, ActionIdentifier), Assert.Single(args.Notification.Data));
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