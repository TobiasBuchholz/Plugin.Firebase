using Plugin.Firebase.Common;
using Plugin.Firebase.iOS.RemoteConfig;
using FirebaseRemoteConfig = Firebase.RemoteConfig.RemoteConfig;

namespace Plugin.Firebase.RemoteConfig;

public sealed class FirebaseRemoteConfigImplementation : DisposableBase, IFirebaseRemoteConfig
{
    private readonly FirebaseRemoteConfig _remoteConfig;

    public FirebaseRemoteConfigImplementation()
    {
        _remoteConfig = FirebaseRemoteConfig.SharedInstance;
    }

    public Task EnsureInitializedAsync()
    {
        return _remoteConfig.EnsureInitializedAsync();
    }

    public Task SetRemoteConfigSettingsAsync(RemoteConfigSettings configSettings)
    {
        _remoteConfig.ConfigSettings = configSettings.ToNative();
        return Task.CompletedTask;
    }

    public Task SetDefaultsAsync(IDictionary<string, object> defaults)
    {
        _remoteConfig.SetDefaults(defaults.ToNSDictionary());
        return Task.CompletedTask;
    }

    public Task SetDefaultsAsync(params (string, object)[] defaults)
    {
        _remoteConfig.SetDefaults(defaults.ToNSDictionary());
        return Task.CompletedTask;
    }

    public Task FetchAndActivateAsync()
    {
        return _remoteConfig.FetchAndActivateAsync();
    }

    public Task FetchAsync(double expirationDuration = 3600)
    {
        return _remoteConfig.FetchAsync(expirationDuration);
    }

    public Task ActivateAsync()
    {
        return _remoteConfig.ActivateAsync();
    }

    public IEnumerable<string> GetKeysByPrefix(string prefix)
    {
        return _remoteConfig.GetKeys(prefix).ToArray().Select(x => (string) x);
    }

    public bool GetBoolean(string key) => _remoteConfig.GetConfigValue(key).BoolValue;
    public string GetString(string key) => _remoteConfig.GetConfigValue(key).StringValue;
    public long GetLong(string key) => (long) _remoteConfig.GetConfigValue(key).NumberValue;
    public double GetDouble(string key) => (double) _remoteConfig.GetConfigValue(key).NumberValue;

    public RemoteConfigInfo Info =>
        new RemoteConfigInfo(
            _remoteConfig.ConfigSettings.ToAbstract(),
            DateTimeOffset.FromUnixTimeSeconds((long) (_remoteConfig.LastFetchTime?.SecondsSince1970 ?? 0)),
            (RemoteConfigFetchStatus) (long) _remoteConfig.LastFetchStatus);
}