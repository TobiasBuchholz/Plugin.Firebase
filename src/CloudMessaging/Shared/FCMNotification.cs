namespace Plugin.Firebase.CloudMessaging
{
    /// <summary>
    /// Represents a Firebase Cloud Messaging notification.
    /// </summary>
    public sealed class FCMNotification
    {
        /// <summary>
        /// Creates an empty notification instance.
        /// </summary>
        /// <returns>An empty <c>FCMNotification</c>.</returns>
        public static FCMNotification Empty()
        {
            return new FCMNotification();
        }

        private readonly string _body;
        private readonly string _title;

        /// <summary>
        /// Creates a new <c>FCMNotification</c> instance.
        /// </summary>
        /// <param name="body">The notification body text.</param>
        /// <param name="title">The notification title.</param>
        /// <param name="imageUrl">The URL of an image to display in the notification.</param>
        /// <param name="data">Additional data payload as key-value pairs.</param>
        public FCMNotification(
            string body = null,
            string title = null,
            string imageUrl = null,
            IDictionary<string, string> data = null
        )
        {
            _body = body;
            _title = title;
            ImageUrl = imageUrl;
            Data = data;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"[FCMNotification: Body={Body}, Title={Title}, Data={(Data == null ? "" : string.Join(", ", Data.Select(kvp => $"{kvp.Key}:{kvp.Value}")))}]";
        }

        /// <summary>
        /// Gets the notification body text.
        /// </summary>
        public string Body =>
            _body ?? (Data != null && Data.ContainsKey("body") ? Data["body"] : "");

        /// <summary>
        /// Gets the notification title.
        /// </summary>
        public string Title =>
            _title ?? (Data != null && Data.ContainsKey("title") ? Data["title"] : "");

        /// <summary>
        /// Gets whether the notification should be silent in the foreground.
        /// </summary>
        public bool IsSilentInForeground =>
            Data != null
            && Data.ContainsKey("is_silent_in_foreground")
            && bool.TryParse(Data["is_silent_in_foreground"], out var value)
            && value;

        /// <summary>
        /// Gets the URL of an image to display in the notification.
        /// </summary>
        public string ImageUrl { get; }

#nullable enable
        /// <summary>
        /// Gets the Android notification channel ID specified by the sender.
        /// </summary>
        /// <remarks>
        /// This value is available only for Android notification messages. It is
        /// <see langword="null"/> when the sender did not specify a channel or on other platforms.
        /// The value is not validated and does not include Firebase's default-channel fallback.
        /// </remarks>
        public string? ChannelId { get; init; }
#nullable restore

        /// <summary>
        /// Gets the additional data payload as key-value pairs.
        /// </summary>
        public IDictionary<string, string> Data { get; }
    }
}