using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.RemoteConfig
{
    public interface IFirebaseRemoteConfig : IDisposable
    {
        Task EnsureInitializedAsync();
        Task SetRemoteConfigSettingsAsync(RemoteConfigSettings configSettings);
        Task SetDefaultsAsync(IDictionary<string, object> defaults);
        Task SetDefaultsAsync(params (string, object)[] defaults);
        Task FetchAndActivateAsync();
        Task FetchAsync(double expirationDuration = 3600);
        Task ActivateAsync();
        IEnumerable<string> GetKeysByPrefix(string prefix);
        bool GetBoolean(string key);
        string GetString(string key);
        long GetLong(string key);
        double GetDouble(string key);
        
        RemoteConfigInfo Info { get; }
    }
}