using Genesis.Logging;
using Playground.Common.Services.Helper;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;
// using Plugin.Firebase.Functions;

namespace Playground.Common.Services.PushNotification;

public sealed class PushNotificationService : IPushNotificationService
{
    private readonly IFirebaseCloudMessaging _firebaseCloudMessaging;
    // private readonly IFirebaseFunctions _firebaseFunctions;
    private readonly ILogger _logger;

    public PushNotificationService(
        IFirebaseCloudMessaging firebaseCloudMessaging,
        // IFirebaseFunctions firebaseFunctions,
        ILogger logger)
    {
        _firebaseCloudMessaging = firebaseCloudMessaging;
        // _firebaseFunctions = firebaseFunctions;
        _logger = logger;
        NotificationTapped = GetNotificationTappedTicks();
    }

    private IObservable<FCMNotification> GetNotificationTappedTicks()
    {
        return Observable
            .FromEventPattern<FCMNotificationTappedEventArgs>(_firebaseCloudMessaging, nameof(_firebaseCloudMessaging.NotificationTapped))
            .Select(x => x.EventArgs.Notification);
    }

    public Task CheckIfValidAsync()
    {
        return _firebaseCloudMessaging.CheckIfValidAsync();
    }

    public Task<string> GetFcmTokenAsync()
    {
        return _firebaseCloudMessaging.GetTokenAsync();
    }

    public Task SubscribeToTopicAsync(string topic)
    {
        return _firebaseCloudMessaging.SubscribeToTopicAsync(topic);
    }

    public Task UnsubscribeFromTopicAsync(string topic)
    {
        return _firebaseCloudMessaging.UnsubscribeFromTopicAsync(topic);
    }

    public Task TriggerNotificationViaTokensAsync(IEnumerable<string> tokens, string title, string body)
    {
        return Task.CompletedTask;
        
        // return _firebaseFunctions
        //     .GetHttpsCallable(FirebaseFunctionNames.TriggerNotification)
        //     .CallAsync(PushNotification.FromTokens(tokens, title, body).ToJson());
    }

    public Task TriggerNotificationViaTopicAsync(string topic, string title, string body)
    {
        return Task.CompletedTask;
        
        // return _firebaseFunctions
        //     .GetHttpsCallable(FirebaseFunctionNames.TriggerNotification)
        //     .CallAsync(PushNotification.FromTopic(topic, title, body).ToJson());
    }

    public IObservable<FCMNotification> NotificationTapped { get; }
}