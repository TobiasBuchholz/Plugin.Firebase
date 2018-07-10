using System;
using System.Collections.Generic;

namespace Plugin.Firebase.Abstractions.Analytics
{
    public interface IFirebaseAnalytics : IDisposable
    {
        void LogEvent(string eventName, IDictionary<string, object> parameters);
        void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters);
    }
}