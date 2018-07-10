using System.Collections.Generic;
using System.Linq;
using Plugin.Firebase.Abstractions;
using Plugin.Firebase.Abstractions.Analytics;
using SDK = Firebase.Analytics;

namespace Plugin.Firebase.Analytics
{
    public sealed class FirebaseAnalyticsImplementation : DisposableBase, IFirebaseAnalytics
    {
        public void LogEvent(string eventName, IDictionary<string, object> parameters)
        {
            SDK.Analytics.LogEvent(eventName, parameters?.ToNSDictionary());
        }
        
        public void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters)
        {
            LogEvent(eventName, parameters?.ToDictionary(x => x.parameterName, x => x.parameterValue));
        }
    }
}