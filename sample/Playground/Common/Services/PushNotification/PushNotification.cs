using Microsoft.Maui.Controls.Internals;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Playground.Common.Services.PushNotification;

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
        return JsonSerializer.Serialize(this);
    }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PushNotificationType Type { get; private set; }

    [JsonPropertyName("title")]
    public string Title { get; private set; }

    [JsonPropertyName("body")]
    public string Body { get; private set; }

    [JsonPropertyName("fcm_tokens")]
    public IEnumerable<string> FcmTokens { get; private set; }

    [JsonPropertyName("topic")]
    public string Topic { get; private set; }
}