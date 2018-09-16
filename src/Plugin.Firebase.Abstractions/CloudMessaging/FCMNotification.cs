using System.Collections.Generic;
using System.Linq;

namespace Plugin.Firebase.Abstractions.CloudMessaging
{
    public sealed class FCMNotification
    {
        public static FCMNotification Empty()
        {
            return new FCMNotification();
        }
        
        public FCMNotification(
            string body = null, 
            string title = null, 
            IDictionary<string, string> data = null)
        {
            Body = body;
            Title = title;
            Data = data;
        }

        public override string ToString()
        {
            return string.Format($"[FCMNotification: Body={Body}, Title={Title}, Data=") + string.Join(", ", Data?.Select(kvp => $"{kvp.Key}:{kvp.Value}")) + "]";
        }
        
        public string Body { get; }
        public string Title { get; }
        public IDictionary<string, string> Data { get; }
    }
}