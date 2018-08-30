using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Firebase.Analytics;
using Plugin.Firebase.Abstractions;
using Plugin.Firebase.Abstractions.Analytics;
using Plugin.Firebase.Abstractions.Common;

namespace Plugin.Firebase.Analytics
{
    public sealed class FirebaseAnalyticsImplementation : DisposableBase, IFirebaseAnalytics
    {
        public static void Initialize(Context context)
        {
            _firebaseAnalytics = FirebaseAnalytics.GetInstance(context);     
        }
        
        private static FirebaseAnalytics _firebaseAnalytics;
        
        public void LogEvent(string eventName, IDictionary<string, object> parameters)
        {
            _firebaseAnalytics.LogEvent(eventName, parameters?.ToBundle());
        }

        public void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters)
        {
            LogEvent(eventName, parameters?.ToDictionary(x => x.parameterName, x => x.parameterValue));
        }

        public bool IsAnalyticsCollectionEnabled {
            set => _firebaseAnalytics.SetAnalyticsCollectionEnabled(value);
        }
    }
}