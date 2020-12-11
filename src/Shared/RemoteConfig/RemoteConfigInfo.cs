using System;

namespace Plugin.Firebase.RemoteConfig
{
    public sealed class RemoteConfigInfo
    {
        public RemoteConfigInfo(RemoteConfigSettings configSettings, DateTimeOffset lastFetchTime, RemoteConfigFetchStatus lastFetchStatus)
        {
            ConfigSettings = configSettings;
            LastFetchTime = lastFetchTime;
            LastFetchStatus = lastFetchStatus;
        }

        public RemoteConfigSettings ConfigSettings { get; }
        public DateTimeOffset LastFetchTime { get; }
        public RemoteConfigFetchStatus LastFetchStatus { get; }
    }
}