using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using Plugin.Firebase.Common;
using FirebaseRemoteConfig = Firebase.RemoteConfig.RemoteConfig;

namespace Plugin.Firebase.RemoteConfig
{
    public sealed class FirebaseRemoteConfigImplementation : DisposableBase, IFirebaseRemoteConfig
    {
        private readonly FirebaseRemoteConfig _remoteConfig;

        public FirebaseRemoteConfigImplementation()
        {
            _remoteConfig = FirebaseRemoteConfig.SharedInstance;
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

        public Task FetchAndActivateAsync(double expirationDuration = 43200)
        {
            _remoteConfig.ConfigSettings = new RemoteConfigSettings { MinimumFetchInterval = expirationDuration };
            return _remoteConfig.FetchAndActivateAsync();
        }

        public Task FetchAsync(double expirationDuration = 43200)
        {
            return _remoteConfig.FetchAsync(expirationDuration);
        }

        public Task ActivateAsync()
        {
            return _remoteConfig.ActivateAsync();
        }

        public bool GetBoolean(string key) => _remoteConfig.GetConfigValue(key).BoolValue;
        public string GetString(string key) => _remoteConfig.GetConfigValue(key).StringValue;
        public long GetLong(string key) => (long) _remoteConfig.GetConfigValue(key).NumberValue;
        public double GetDouble(string key) => (double) _remoteConfig.GetConfigValue(key).NumberValue;
    }
}