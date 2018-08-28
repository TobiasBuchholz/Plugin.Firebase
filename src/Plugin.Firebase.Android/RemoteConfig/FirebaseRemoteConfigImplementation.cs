using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using Plugin.Firebase.Abstractions.Common;
using Plugin.Firebase.Abstractions.RemoteConfig;

namespace Plugin.Firebase.RemoteConfig
{
    public sealed class FirebaseRemoteConfigImplementation : DisposableBase, IFirebaseRemoteConfig
    {
        private readonly FirebaseRemoteConfig _remoteConfig;

        public FirebaseRemoteConfigImplementation()
        {
            _remoteConfig = FirebaseRemoteConfig.Instance;
        }

        public void SetDefaults(IDictionary<string, object> defaults)
        {
            _remoteConfig.SetDefaults(defaults.ToJavaObjectDictionary());
        }
        
        public void SetDefaults(params (string, object)[] defaults)
        {
            _remoteConfig.SetDefaults(defaults.ToJavaObjectDictionary());
        }

        public void SetDeveloperModeEnabled(bool isEnabled)
        {
            _remoteConfig.SetConfigSettings(new FirebaseRemoteConfigSettings.Builder().SetDeveloperModeEnabled(isEnabled).Build());
        }

        public Task FetchAsync(double expirationDuration = 43200)
        {
            return _remoteConfig.FetchAsync((long) expirationDuration);
        }

        public void ActivateFetched()
        {
            _remoteConfig.ActivateFetched();
        }

        public bool GetBoolean(string key) => _remoteConfig.GetBoolean(key);
        public string GetString(string key) => _remoteConfig.GetString(key);
        public long GetLong(string key) => _remoteConfig.GetLong(key);
        public double GetDouble(string key) => _remoteConfig.GetDouble(key);
        public byte[] GetByteArray(string key) => _remoteConfig.GetByteArray(key); 
    }
}