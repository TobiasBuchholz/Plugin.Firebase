using Plugin.Firebase.Core;
using Plugin.Firebase.Crashlytics.Platforms.iOS;
using FirebaseCrashlytics = Firebase.Crashlytics.Crashlytics;

namespace Plugin.Firebase.Crashlytics;

/// <summary>
/// iOS implementation of <see cref="IFirebaseCrashlytics"/> that wraps the native Firebase Crashlytics SDK.
/// </summary>
public sealed class FirebaseCrashlyticsImplementation : DisposableBase, IFirebaseCrashlytics
{
    private readonly FirebaseCrashlytics _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="FirebaseCrashlyticsImplementation"/> class.
    /// </summary>
    public FirebaseCrashlyticsImplementation()
    {
        _instance = FirebaseCrashlytics.SharedInstance;
    }

    /// <inheritdoc/>
    public void SetCrashlyticsCollectionEnabled(bool enabled)
    {
        _instance.SetCrashlyticsCollectionEnabled(enabled);
    }

    /// <inheritdoc/>
    public void SetCustomKey(string key, bool value)
    {
        _instance.SetCustomValue(new NSNumber(value), key);
    }

    /// <inheritdoc/>
    public void SetCustomKey(string key, int value)
    {
        _instance.SetCustomValue(new NSNumber(value), key);
    }

    /// <inheritdoc/>
    public void SetCustomKey(string key, long value)
    {
        _instance.SetCustomValue(new NSNumber(value), key);
    }

    /// <inheritdoc/>
    public void SetCustomKey(string key, float value)
    {
        _instance.SetCustomValue(new NSNumber(value), key);
    }

    /// <inheritdoc/>
    public void SetCustomKey(string key, double value)
    {
        _instance.SetCustomValue(new NSNumber(value), key);
    }

    /// <inheritdoc/>
    public void SetCustomKey(string key, string value)
    {
        _instance.SetCustomValue(new NSString(value), key);
    }

    /// <inheritdoc/>
    public void SetCustomKeys(IDictionary<string, object> customKeysAndValues)
    {
        var nsDict = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
            customKeysAndValues.Values.ToArray(),
            customKeysAndValues.Keys.ToArray<object>()
        );
        _instance.SetCustomKeysAndValues(nsDict);
    }

    /// <inheritdoc/>
    public void SetUserId(string identifier)
    {
        _instance.SetUserId(identifier);
    }

    /// <inheritdoc/>
    public void Log(string message)
    {
        _instance.Log(message);
    }

    /// <inheritdoc/>
    public void RecordException(Exception exception)
    {
        _instance.RecordExceptionModel(CrashlyticsException.Create(exception));
    }

    /// <inheritdoc/>
    public bool DidCrashOnPreviousExecution()
    {
        return _instance.DidCrashDuringPreviousExecution;
    }

    /// <inheritdoc/>
    public Task<bool> CheckForUnsentReportsAsync()
    {
        return _instance.CheckForUnsentReportsAsync();
    }

    /// <inheritdoc/>
    public void SendUnsentReports()
    {
        _instance.SendUnsentReports();
    }

    /// <inheritdoc/>
    public void DeleteUnsentReports()
    {
        _instance.DeleteUnsentReports();
    }

    //void HandleUncaughtException(bool shouldThrowFormattedException = true);
}