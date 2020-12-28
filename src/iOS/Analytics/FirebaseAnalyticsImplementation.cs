using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Firebase.Common;
using Plugin.Firebase.iOS.Extensions;
using FirebaseAnalytics = Firebase.Analytics.Analytics;

namespace Plugin.Firebase.Analytics
{
    public sealed class FirebaseAnalyticsImplementation : DisposableBase, IFirebaseAnalytics
    {
        public static void Initialize()
        {
            // does nothing but still used for consistency across all features
        }

        public Task<string> GetAppInstanceIdAsync()
        {
            return Task.FromResult(FirebaseAnalytics.AppInstanceId);
        }
        
        public void LogEvent(string eventName, IDictionary<string, object> parameters)
        {
            FirebaseAnalytics.LogEvent(eventName, parameters?.ToNSDictionary());
        }
        
        public void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters)
        {
            LogEvent(eventName, parameters?.ToDictionary(x => x.parameterName, x => x.parameterValue));
        }

        public void SetUserId(string id)
        {
            FirebaseAnalytics.SetUserId(id);
        }

        public void SetUserProperty(string name, string value)
        {
            FirebaseAnalytics.SetUserProperty(value, name);
        }

        public void SetSessionTimoutDuration(TimeSpan duration)
        {
            FirebaseAnalytics.SetSessionTimeoutInterval(duration.TotalSeconds);
        }

        public void ResetAnalyticsData()
        {
            FirebaseAnalytics.ResetAnalyticsData();
        }

        public bool IsAnalyticsCollectionEnabled {
            set => FirebaseAnalytics.SetAnalyticsCollectionEnabled(value);
        }
    }
}