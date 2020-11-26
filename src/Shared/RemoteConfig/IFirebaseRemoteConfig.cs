using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.RemoteConfig
{
    public interface IFirebaseRemoteConfig : IDisposable
    {
        Task SetDefaultsAsync(IDictionary<string, object> defaults);
        Task SetDefaultsAsync(params (string, object)[] defaults);
        Task FetchAndActivateAsync(double expirationDuration = 43200);
        Task FetchAsync(double expirationDuration = 43200);
        Task ActivateAsync();
        bool GetBoolean(string key);
        string GetString(string key);
        long GetLong(string key);
        double GetDouble(string key);
    }
}