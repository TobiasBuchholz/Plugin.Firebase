using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.RemoteConfig;
using Plugin.Firebase.Abstractions.Common;
using Plugin.Firebase.Abstractions.RemoteConfig;
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

        public void SetDefaults(IDictionary<string, object> defaults)
        {
            _remoteConfig.SetDefaults(defaults.ToNSDictionary());
        }

        public void SetDefaults(params (string, object)[] defaults)
        {
            _remoteConfig.SetDefaults(defaults.ToNSDictionary());
        }

        public void SetDeveloperModeEnabled(bool isEnabled)
        {
            _remoteConfig.ConfigSettings = new RemoteConfigSettings(isEnabled);
        }

        public Task FetchAsync(double expirationDuration = 43200)
        {
            return _remoteConfig.FetchAsync(expirationDuration);
        }

        public void ActivateFetched()
        {
            _remoteConfig.ActivateFetched();
        }

        public bool GetBoolean(string key) => _remoteConfig.GetConfigValue(key).BoolValue;
        public string GetString(string key) => _remoteConfig.GetConfigValue(key).StringValue;
        public long GetLong(string key) => (long) _remoteConfig.GetConfigValue(key).NumberValue;
        public double GetDouble(string key) => (double) _remoteConfig.GetConfigValue(key).NumberValue;
        public byte[] GetByteArray(string key) => _remoteConfig.GetConfigValue(key).DataValue.ToArray(); 
    }
}