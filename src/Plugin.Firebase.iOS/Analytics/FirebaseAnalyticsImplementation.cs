using System.Collections.Generic;
using System.Linq;
using Plugin.Firebase.Abstractions.Analytics;
using Plugin.Firebase.Abstractions.Common;
using FirebaseAnalytics = Firebase.Analytics.Analytics;
using FirebaseAnalyticsConfiguration = Firebase.Core.AnalyticsConfiguration;

namespace Plugin.Firebase.Analytics
{
    public sealed class FirebaseAnalyticsImplementation : DisposableBase, IFirebaseAnalytics
    {
        public static void Initialize()
        {
            // does nothing but still used for consistency accross all features
        }
        
        public void LogEvent(string eventName, IDictionary<string, object> parameters)
        {
            FirebaseAnalytics.LogEvent(eventName, parameters?.ToNSDictionary());
        }
        
        public void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters)
        {
            LogEvent(eventName, parameters?.ToDictionary(x => x.parameterName, x => x.parameterValue));
        }

        public bool IsAnalyticsCollectionEnabled {
            set => FirebaseAnalyticsConfiguration.SharedInstance.SetAnalyticsCollectionEnabled(value);
        }
    }
}