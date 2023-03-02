using Foundation;
using Plugin.Firebase.Core;
using Plugin.Firebase.Crashlytics.Platforms.iOS;
using FirebaseCrashlytics = Firebase.Crashlytics.Crashlytics;

namespace Plugin.Firebase.Crashlytics;

public sealed class FirebaseCrashlyticsImplementation : DisposableBase, IFirebaseCrashlytics
{
    private readonly FirebaseCrashlytics _instance;

    public FirebaseCrashlyticsImplementation()
    {
        _instance = FirebaseCrashlytics.SharedInstance;
    }

    public void SetCrashlyticsCollectionEnabled(bool enabled)
    {
        _instance.SetCrashlyticsCollectionEnabled(enabled);
    }

    public void SetCustomKey(string key, bool value)
    {
        _instance.SetCustomValue(new NSNumber(value), key);
    }

    public void SetCustomKey(string key, int value)
    {
        _instance.SetCustomValue(new NSNumber(value), key);
    }

    public void SetCustomKey(string key, long value)
    {
        _instance.SetCustomValue(new NSNumber(value), key);
    }

    public void SetCustomKey(string key, float value)
    {
        _instance.SetCustomValue(new NSNumber(value), key);
    }

    public void SetCustomKey(string key, double value)
    {
        _instance.SetCustomValue(new NSNumber(value), key);
    }

    public void SetCustomKey(string key, string value)
    {
        _instance.SetCustomValue(new NSString(value), key);
    }

    public void SetCustomKeys(IDictionary<string, object> customKeysAndValues)
    {
        var nsDict = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(customKeysAndValues.Values.ToArray(), customKeysAndValues.Keys.ToArray<object>());
        _instance.SetCustomKeysAndValues(nsDict);
    }

    public void SetUserId(string identifier)
    {
        _instance.SetUserId(identifier);
    }

    public void Log(string message)
    {
        _instance.Log(message);
    }

    public void RecordException(Exception exception)
    {
        _instance.RecordExceptionModel(CrashlyticsException.Create(exception));
    }

    public bool DidCrashOnPreviousExecution()
    {
        return _instance.DidCrashDuringPreviousExecution;
    }

    public Task<bool> CheckForUnsentReportsAsync()
    {
        return _instance.CheckForUnsentReportsAsync();
    }

    public void SendUnsentReports()
    {
        _instance.SendUnsentReports();
    }

    public void DeleteUnsentReports()
    {
        _instance.DeleteUnsentReports();
    }

    //void HandleUncaughtException(bool shouldThrowFormattedException = true);
}