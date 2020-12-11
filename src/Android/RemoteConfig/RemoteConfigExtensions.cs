using System;
using Firebase.RemoteConfig;
using Plugin.Firebase.Common;
using Plugin.Firebase.RemoteConfig;
using NativeRemoteConfigSettings = Firebase.RemoteConfig.FirebaseRemoteConfigSettings;

namespace Plugin.Firebase.Android.RemoteConfig
{
    public static class RemoteConfigExtensions
    {
        public static NativeRemoteConfigSettings ToNative(this RemoteConfigSettings @this)
        {
            return new NativeRemoteConfigSettings.Builder()
                .SetMinimumFetchIntervalInSeconds((long) @this.MinimumFetchInterval.TotalSeconds)
                .SetFetchTimeoutInSeconds((long) @this.FetchTimeout.TotalSeconds)
                .Build();
        }

        public static RemoteConfigSettings ToAbstract(this NativeRemoteConfigSettings @this)
        {
            return new RemoteConfigSettings(
                TimeSpan.FromSeconds(@this.MinimumFetchIntervalInSeconds),
                TimeSpan.FromSeconds(@this.FetchTimeoutInSeconds));
        }

        public static RemoteConfigInfo ToAbstract(this IFirebaseRemoteConfigInfo @this)
        {
            return new RemoteConfigInfo(
                @this.ConfigSettings.ToAbstract(),
                DateTimeOffset.FromUnixTimeMilliseconds(@this.FetchTimeMillis),
                ConvertToRemoteConfigFetchStatus(@this.LastFetchStatus));
        }

        private static RemoteConfigFetchStatus ConvertToRemoteConfigFetchStatus(int status)
        {
            switch(status) {
                case -1: 
                    return RemoteConfigFetchStatus.Success;
                case 0: 
                    return RemoteConfigFetchStatus.NoFetchYet;
                case 1: 
                    return RemoteConfigFetchStatus.Failure;
                case 2: 
                    return RemoteConfigFetchStatus.Throttled;
                default:
                    throw new FirebaseException($"Couldn't convert int {status} to {nameof(RemoteConfigFetchStatus)}");
            }
        }
    }
}