using System;
using System.Collections.Generic;

namespace Plugin.Firebase.Analytics
{
    public interface IFirebaseAnalytics : IDisposable
    {
        void LogEvent(string eventName, IDictionary<string, object> parameters);
        void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters);
        
        bool IsAnalyticsCollectionEnabled { set; }
    }
}