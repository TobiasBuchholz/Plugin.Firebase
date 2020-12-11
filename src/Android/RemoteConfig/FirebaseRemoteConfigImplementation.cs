using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Gms.Extensions;
using Firebase.RemoteConfig;
using Plugin.Firebase.Android.RemoteConfig;
using Plugin.Firebase.Common;
using Plugin.Firebase.Extensions;

namespace Plugin.Firebase.RemoteConfig
{
    public sealed class FirebaseRemoteConfigImplementation : DisposableBase, IFirebaseRemoteConfig
    {
        private readonly FirebaseRemoteConfig _remoteConfig;

        public FirebaseRemoteConfigImplementation()
        {
            _remoteConfig = FirebaseRemoteConfig.Instance;
        }

        public Task EnsureInitializedAsync()
        {
            return _remoteConfig.EnsureInitialized().ToTask();
        }

        public Task SetRemoteConfigSettingsAsync(RemoteConfigSettings configSettings)
        {
            return _remoteConfig.SetConfigSettingsAsync(configSettings.ToNative()).ToTask();
        }

        public Task SetDefaultsAsync(IDictionary<string, object> defaults)
        {
            return _remoteConfig.SetDefaultsAsync(defaults.ToJavaObjectDictionary()).ToTask();
        }
        
        public Task SetDefaultsAsync(params (string, object)[] defaults)
        {
            return _remoteConfig.SetDefaultsAsync(defaults.ToJavaObjectDictionary()).ToTask();
        }

        public async Task FetchAndActivateAsync()
        {
            await _remoteConfig.FetchAndActivate();
        }
        
        public Task FetchAsync(double expirationDuration = 3600)
        {
            return _remoteConfig.FetchAsync((long) expirationDuration);
        }

        public Task ActivateAsync()
        {
            return _remoteConfig.Activate().ToTask();
        }

        public IEnumerable<string> GetKeysByPrefix(string prefix)
        {
            return _remoteConfig.GetKeysByPrefix(prefix);
        }

        public bool GetBoolean(string key) => _remoteConfig.GetBoolean(key);
        public string GetString(string key) => _remoteConfig.GetString(key);
        public long GetLong(string key) => _remoteConfig.GetLong(key);
        public double GetDouble(string key) => _remoteConfig.GetDouble(key);

        public RemoteConfigInfo Info => _remoteConfig.Info.ToAbstract();
    }
}