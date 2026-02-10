using Plugin.Firebase.Analytics.Platforms.iOS.Extensions;
using Plugin.Firebase.Core;
using FirebaseAnalytics = Firebase.Analytics.Analytics;

namespace Plugin.Firebase.Analytics;

/// <summary>
/// iOS implementation of Firebase Analytics that wraps the native Firebase Analytics SDK.
/// </summary>
public sealed class FirebaseAnalyticsImplementation : DisposableBase, IFirebaseAnalytics
{
    /// <inheritdoc/>
    public Task<string> GetAppInstanceIdAsync()
    {
        return Task.FromResult(FirebaseAnalytics.AppInstanceId);
    }

    /// <inheritdoc/>
    public void LogEvent(string eventName, IDictionary<string, object> parameters)
    {
        FirebaseAnalytics.LogEvent(eventName, parameters?.ToNSDictionary());
    }

    /// <inheritdoc/>
    public void LogEvent(
        string eventName,
        params (string parameterName, object parameterValue)[] parameters
    )
    {
        LogEvent(eventName, parameters?.ToDictionary(x => x.parameterName, x => x.parameterValue));
    }

    /// <inheritdoc/>
    public void SetUserId(string id)
    {
        FirebaseAnalytics.SetUserId(id);
    }

    /// <inheritdoc/>
    public void SetUserProperty(string name, string value)
    {
        FirebaseAnalytics.SetUserProperty(value, name);
    }

    /// <inheritdoc/>
    public void SetSessionTimoutDuration(TimeSpan duration)
    {
        FirebaseAnalytics.SetSessionTimeoutInterval(duration.TotalSeconds);
    }

    /// <inheritdoc/>
    public void ResetAnalyticsData()
    {
        FirebaseAnalytics.ResetAnalyticsData();
    }

    /// <inheritdoc/>
    public bool IsAnalyticsCollectionEnabled {
        set => FirebaseAnalytics.SetAnalyticsCollectionEnabled(value);
    }
}