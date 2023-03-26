namespace Plugin.Firebase.CloudMessaging.EventArgs
{
    public sealed class FCMErrorEventArgs : System.EventArgs
    {
        public FCMErrorEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}