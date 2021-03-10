using System.Collections.Generic;
using System.Linq;

namespace Plugin.Firebase.CloudMessaging
{
    public sealed class FCMNotification
    {
        public static FCMNotification Empty()
        {
            return new FCMNotification();
        }

        private readonly string _body;
        private readonly string _title;
        
        public FCMNotification(
            string body = null, 
            string title = null, 
            string imageUrl = null,
            IDictionary<string, string> data = null)
        {
            _body = body;
            _title = title;
            ImageUrl = imageUrl;
            Data = data;
        }

        public override string ToString()
        {
            return $"[FCMNotification: Body={Body}, Title={Title}, Data={(Data == null ? "" : string.Join(", ", Data.Select(kvp => $"{kvp.Key}:{kvp.Value}")))}]";
        }

        public string Body => _body ?? (Data != null && Data.ContainsKey("body") ? Data["body"] : "");
        public string Title => _title ?? (Data != null && Data.ContainsKey("title") ? Data["title"] : "");
        public string ImageUrl { get; }
        public IDictionary<string, string> Data { get; }
    }
}