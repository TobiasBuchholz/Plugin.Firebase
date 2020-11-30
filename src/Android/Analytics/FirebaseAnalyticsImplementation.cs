using System;
using System.Collections.Generic;
//using System.Linq;
using Android.Content;
//using Firebase.Analytics;
using Plugin.Firebase.Common;

namespace Plugin.Firebase.Analytics
{
    public sealed class FirebaseAnalyticsImplementation : DisposableBase, IFirebaseAnalytics
    {
        public static void Initialize(Context context)
        {
            throw new NotImplementedException("Analytics is not implemented on android because of build issues");
            //_firebaseAnalytics = FirebaseAnalytics.GetInstance(context);     
        }
        
        //private static FirebaseAnalytics _firebaseAnalytics;
        
        public void LogEvent(string eventName, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException("Analytics is not implemented on android because of build issues");
            //_firebaseAnalytics.LogEvent(eventName, parameters?.ToBundle());
        }

        public void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters)
        {
            throw new NotImplementedException("Analytics is not implemented on android because of build issues");
            //LogEvent(eventName, parameters?.ToDictionary(x => x.parameterName, x => x.parameterValue));
        }

        public bool IsAnalyticsCollectionEnabled {
            set => throw new NotImplementedException("Analytics is not implemented on android because of build issues");
            // set => _firebaseAnalytics.SetAnalyticsCollectionEnabled(value);
        }
    }
}