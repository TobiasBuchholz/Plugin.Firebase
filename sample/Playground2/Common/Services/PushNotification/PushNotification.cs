using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Playground.Common.Services.PushNotification
{
    [Preserve(AllMembers = true)]
    public sealed class PushNotification
    {
        public static PushNotification FromTokens(IEnumerable<string> fcmTokens, string title, string body)
        {
            return new PushNotification(PushNotificationType.Tokens, fcmTokens: fcmTokens, title: title, body: body);
        }

        public static PushNotification FromTopic(string topic, string title, string body)
        {
            return new PushNotification(PushNotificationType.Topic, topic: topic, title: title, body: body);
        }

        private PushNotification(
            PushNotificationType type,
            IEnumerable<string> fcmTokens = null,
            string topic = null,
            string title = null,
            string body = null)
        {
            Type = type;
            FcmTokens = fcmTokens;
            Topic = topic;
            Title = title;
            Body = body;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public PushNotificationType Type { get; private set; }

        [JsonProperty("title")]
        public string Title { get; private set; }

        [JsonProperty("body")]
        public string Body { get; private set; }

        [JsonProperty("fcm_tokens")]
        public IEnumerable<string> FcmTokens { get; private set; }

        [JsonProperty("topic")]
        public string Topic { get; private set; }
    }
}