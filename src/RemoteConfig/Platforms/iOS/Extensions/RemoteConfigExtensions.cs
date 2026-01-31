using NativeRemoteConfigSettings = Firebase.RemoteConfig.RemoteConfigSettings;

namespace Plugin.Firebase.RemoteConfig.Platforms.iOS.Extensions;

/// <summary>
/// Provides extension methods for converting between abstract and native iOS Firebase Remote Config settings.
/// </summary>
public static class RemoteConfigExtensions
{
    /// <summary>
    /// Converts abstract RemoteConfigSettings to native iOS Firebase RemoteConfigSettings.
    /// </summary>
    /// <param name="this">The abstract settings to convert.</param>
    /// <returns>Native iOS Firebase RemoteConfigSettings.</returns>
    public static NativeRemoteConfigSettings ToNative(this RemoteConfigSettings @this)
    {
        return new NativeRemoteConfigSettings {
            MinimumFetchInterval = @this.MinimumFetchInterval.TotalSeconds,
            FetchTimeout = @this.FetchTimeout.TotalSeconds,
        };
    }

    /// <summary>
    /// Converts native iOS Firebase RemoteConfigSettings to abstract RemoteConfigSettings.
    /// </summary>
    /// <param name="this">The native settings to convert.</param>
    /// <returns>Abstract RemoteConfigSettings.</returns>
    public static RemoteConfigSettings ToAbstract(this NativeRemoteConfigSettings @this)
    {
        return new RemoteConfigSettings(
            TimeSpan.FromSeconds(@this.MinimumFetchInterval),
            TimeSpan.FromSeconds(@this.FetchTimeout)
        );
    }
}