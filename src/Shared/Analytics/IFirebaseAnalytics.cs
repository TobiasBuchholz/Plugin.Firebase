using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Plugin.Firebase.Analytics
{
    public interface IFirebaseAnalytics : IDisposable
    {
        Task<string> GetAppInstanceIdAsync();
        
        void LogEvent(string eventName, IDictionary<string, object> parameters);
        void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters);

        void SetUserId(string id);
        void SetUserProperty(string name, string value);
        void SetCurrentScreen(string screenName, string screenClassOverride);
        void SetSessionTimoutDuration(TimeSpan duration);
        void ResetAnalyticsData();
        
        bool IsAnalyticsCollectionEnabled { set; }
    }
}