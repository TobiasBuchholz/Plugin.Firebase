using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.Abstractions.RemoteConfig
{
    public interface IFirebaseRemoteConfig : IDisposable
    {
        void SetDefaults(IDictionary<string, object> defaults);
        void SetDefaults(params (string, object)[] defaults);
        void SetDeveloperModeEnabled(bool isEnabled);
        Task FetchAsync(double expirationDuration = 43200);
        void ActivateFetched();
        bool GetBoolean(string key);
        string GetString(string key);
        long GetLong(string key);
        double GetDouble(string key);
        byte[] GetByteArray(string key);
    }
}