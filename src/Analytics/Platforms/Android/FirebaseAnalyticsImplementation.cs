using Android.Content;
using Android.Gms.Extensions;
using Firebase.Analytics;
using Plugin.Firebase.Analytics.Android.Extensions;
using Plugin.Firebase.Common;

namespace Plugin.Firebase.Analytics;

public sealed class FirebaseAnalyticsImplementation : DisposableBase, IFirebaseAnalytics
{
    public static void Initialize(Context context)
    {
        _firebaseAnalytics = FirebaseAnalytics.GetInstance(context);
    }

    private static FirebaseAnalytics _firebaseAnalytics;

    public async Task<string> GetAppInstanceIdAsync()
    {
        return (string) await _firebaseAnalytics.GetAppInstanceId().AsAsync<Java.Lang.String>();
    }

    public void LogEvent(string eventName, IDictionary<string, object> parameters)
    {
        _firebaseAnalytics.LogEvent(eventName, parameters?.ToBundle());
    }

    public void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters)
    {
        LogEvent(eventName, parameters?.ToDictionary(x => x.parameterName, x => x.parameterValue));
    }

    public void SetUserId(string id)
    {
        _firebaseAnalytics.SetUserId(id);
    }

    public void SetUserProperty(string name, string value)
    {
        _firebaseAnalytics.SetUserProperty(name, value);
    }

    public void SetSessionTimoutDuration(TimeSpan duration)
    {
        _firebaseAnalytics.SetSessionTimeoutDuration((long) duration.TotalMilliseconds);
    }

    public void ResetAnalyticsData()
    {
        _firebaseAnalytics.ResetAnalyticsData();
    }

    public bool IsAnalyticsCollectionEnabled {
        set => _firebaseAnalytics.SetAnalyticsCollectionEnabled(value);
    }
}