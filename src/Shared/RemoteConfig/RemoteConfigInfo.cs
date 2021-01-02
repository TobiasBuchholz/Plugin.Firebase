using System;

namespace Plugin.Firebase.RemoteConfig
{
    /// <summary>
    /// Wraps the current state of the FirebaseRemoteConfig singleton object.
    /// </summary>
    public sealed class RemoteConfigInfo
    {
        public RemoteConfigInfo(RemoteConfigSettings configSettings, DateTimeOffset lastFetchTime, RemoteConfigFetchStatus lastFetchStatus)
        {
            ConfigSettings = configSettings;
            LastFetchTime = lastFetchTime;
            LastFetchStatus = lastFetchStatus;
        }

        /// <summary>
        /// Gets the current settings of the FirebaseRemoteConfig singleton object. 
        /// </summary>
        public RemoteConfigSettings ConfigSettings { get; }
        
        /// <summary>
        /// Gets the timestamp (milliseconds since epoch) of the last successful fetch, regardless of whether the fetch was activated or not. 
        /// </summary>
        public DateTimeOffset LastFetchTime { get; }
        
        /// <summary>
        /// Gets the status of the most recent fetch attempt. 
        /// </summary>
        public RemoteConfigFetchStatus LastFetchStatus { get; }
    }
}