namespace Plugin.Firebase.DynamicLinks.EventArgs
{
    public sealed class DynamicLinkFailedEventArgs : System.EventArgs
    {
        public DynamicLinkFailedEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}