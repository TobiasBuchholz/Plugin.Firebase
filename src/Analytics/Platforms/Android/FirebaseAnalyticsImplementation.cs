using Android.Content;
using Android.Gms.Extensions;
using Firebase.Analytics;
using Plugin.Firebase.Analytics.Platforms.Android.Extensions;
using Plugin.Firebase.Core;

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
        return (string) await GetInitializedAnalytics().GetAppInstanceId().AsAsync<Java.Lang.String>();
    }

    public void LogEvent(string eventName, IDictionary<string, object> parameters)
    {
        GetInitializedAnalytics().LogEvent(eventName, parameters?.ToBundle());
    }

    public void LogEvent(string eventName, params (string parameterName, object parameterValue)[] parameters)
    {
        LogEvent(eventName, parameters?.ToDictionary(x => x.parameterName, x => x.parameterValue));
    }

    public void SetUserId(string id)
    {
        GetInitializedAnalytics().SetUserId(id);
    }

    public void SetUserProperty(string name, string value)
    {
        GetInitializedAnalytics().SetUserProperty(name, value);
    }

    public void SetSessionTimoutDuration(TimeSpan duration)
    {
        GetInitializedAnalytics().SetSessionTimeoutDuration((long) duration.TotalMilliseconds);
    }

    public void ResetAnalyticsData()
    {
        GetInitializedAnalytics().ResetAnalyticsData();
    }

    public bool IsAnalyticsCollectionEnabled {
        set => GetInitializedAnalytics().SetAnalyticsCollectionEnabled(value);
    }

    private static FirebaseAnalytics GetInitializedAnalytics()
    {
        return _firebaseAnalytics ?? throw new InvalidOperationException(
            "Firebase Analytics has not been initialized on Android. "
                + "When using Plugin.Firebase.Analytics directly, call FirebaseAnalyticsImplementation.Initialize(activity) "
                + "after CrossFirebase.Initialize(...). When using the bundled Plugin.Firebase package, enable "
                + "isAnalyticsEnabled: true in CrossFirebaseSettings."
        );
    }
}