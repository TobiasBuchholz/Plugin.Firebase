namespace Plugin.Firebase.Abstractions.CloudMessaging.EventArgs
{
    public sealed class FCMTokenChangedEventArgs : System.EventArgs
    {
        public FCMTokenChangedEventArgs(string token)
        {
            Token = token;
        }

        public string Token { get; }
    }
}