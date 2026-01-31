using Plugin.Firebase.Core;
using Plugin.Firebase.RemoteConfig.Platforms.iOS.Extensions;
using FirebaseRemoteConfig = Firebase.RemoteConfig.RemoteConfig;

namespace Plugin.Firebase.RemoteConfig;

/// <summary>
/// iOS implementation of Firebase Remote Config that wraps the native iOS Firebase Remote Config SDK.
/// </summary>
public sealed class FirebaseRemoteConfigImplementation : DisposableBase, IFirebaseRemoteConfig
{
    private readonly FirebaseRemoteConfig _remoteConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirebaseRemoteConfigImplementation"/> class.
    /// </summary>
    public FirebaseRemoteConfigImplementation()
    {
        _remoteConfig = FirebaseRemoteConfig.SharedInstance;
    }

    /// <inheritdoc/>
    public Task EnsureInitializedAsync()
    {
        return _remoteConfig.EnsureInitializedAsync();
    }

    /// <inheritdoc/>
    public Task SetRemoteConfigSettingsAsync(RemoteConfigSettings configSettings)
    {
        _remoteConfig.ConfigSettings = configSettings.ToNative();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SetDefaultsAsync(IDictionary<string, object> defaults)
    {
        _remoteConfig.SetDefaults(defaults.ToNSDictionary());
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SetDefaultsAsync(params (string, object)[] defaults)
    {
        _remoteConfig.SetDefaults(defaults.ToNSDictionary());
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task FetchAndActivateAsync()
    {
        return _remoteConfig.FetchAndActivateAsync();
    }

    /// <inheritdoc/>
    public Task FetchAsync(double expirationDuration = 3600)
    {
        return _remoteConfig.FetchAsync(expirationDuration);
    }

    /// <inheritdoc/>
    public Task ActivateAsync()
    {
        return _remoteConfig.ActivateAsync();
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetKeysByPrefix(string prefix)
    {
        return _remoteConfig.GetKeys(prefix).ToArray().Select(x => (string) x);
    }

    /// <inheritdoc/>
    public bool GetBoolean(string key) => _remoteConfig.GetConfigValue(key).BoolValue;

    /// <inheritdoc/>
    public string GetString(string key) => _remoteConfig.GetConfigValue(key).StringValue;

    /// <inheritdoc/>
    public long GetLong(string key) => (long) _remoteConfig.GetConfigValue(key).NumberValue;

    /// <inheritdoc/>
    public double GetDouble(string key) => (double) _remoteConfig.GetConfigValue(key).NumberValue;

    /// <inheritdoc/>
    public RemoteConfigInfo Info =>
        new RemoteConfigInfo(
            _remoteConfig.ConfigSettings.ToAbstract(),
            DateTimeOffset.FromUnixTimeSeconds(
                (long) (_remoteConfig.LastFetchTime?.SecondsSince1970 ?? 0)
            ),
            (RemoteConfigFetchStatus) (long) _remoteConfig.LastFetchStatus
        );
}