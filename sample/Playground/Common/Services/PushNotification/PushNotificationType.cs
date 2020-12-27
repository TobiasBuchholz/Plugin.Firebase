using System.Runtime.Serialization;

namespace Playground.Common.Services.PushNotification
{
    public enum PushNotificationType
    {
        [EnumMember(Value = "TOKENS")]
        Tokens, 
        
        [EnumMember(Value = "TOPIC")]
        Topic
    }
}