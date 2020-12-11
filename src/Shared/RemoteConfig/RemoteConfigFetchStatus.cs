namespace Plugin.Firebase.RemoteConfig
{
    public enum RemoteConfigFetchStatus : long
    {
        NoFetchYet,
        Success,
        Failure,
        Throttled
    }
}