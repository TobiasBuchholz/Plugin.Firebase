using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Firebase.CloudMessaging;

namespace Playground.Common.Services.PushNotification
{
    public interface IPushNotificationService
    {
        Task CheckIfValidAsync();
        Task<string> GetFcmTokenAsync();
        Task SubscribeToTopicAsync(string topic);
        Task UnsubscribeFromTopicAsync(string topic);
        Task TriggerNotificationViaTokensAsync(IEnumerable<string> tokens, string title, string body);
        Task TriggerNotificationViaTopicAsync(string topic, string title, string body);

        IObservable<FCMNotification> NotificationTapped { get; }
    }
}