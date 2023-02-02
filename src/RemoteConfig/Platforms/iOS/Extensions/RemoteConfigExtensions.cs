using Plugin.Firebase.RemoteConfig;
using NativeRemoteConfigSettings = Firebase.RemoteConfig.RemoteConfigSettings;

namespace Plugin.Firebase.iOS.RemoteConfig;

public static class RemoteConfigExtensions
{
    public static NativeRemoteConfigSettings ToNative(this RemoteConfigSettings @this)
    {
        return new NativeRemoteConfigSettings {
            MinimumFetchInterval = @this.MinimumFetchInterval.TotalSeconds,
            FetchTimeout = @this.FetchTimeout.TotalSeconds
        };
    }

    public static RemoteConfigSettings ToAbstract(this NativeRemoteConfigSettings @this)
    {
        return new RemoteConfigSettings(
            TimeSpan.FromSeconds(@this.MinimumFetchInterval),
            TimeSpan.FromSeconds(@this.FetchTimeout));
    }
}