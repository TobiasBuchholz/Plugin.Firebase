using Android.Gms.Extensions;
using Firebase.Crashlytics;
using Plugin.Firebase.Core;
using Plugin.Firebase.Crashlytics.Platforms.Android;

namespace Plugin.Firebase.Crashlytics;

public sealed class FirebaseCrashlyticsImplementation : DisposableBase, IFirebaseCrashlytics
{
    private readonly FirebaseCrashlytics _instance;

    public FirebaseCrashlyticsImplementation()
    {
        _instance = FirebaseCrashlytics.Instance;
    }

    public void SetCrashlyticsCollectionEnabled(bool enabled)
    {
        _instance.SetCrashlyticsCollectionEnabled(enabled);
    }

    public void SetCustomKey(string key, bool value)
    {
        _instance.SetCustomKey(key, value);
    }

    public void SetCustomKey(string key, int value)
    {
        _instance.SetCustomKey(key, value);
    }

    public void SetCustomKey(string key, long value)
    {
        _instance.SetCustomKey(key, value);
    }

    public void SetCustomKey(string key, float value)
    {
        _instance.SetCustomKey(key, value);
    }

    public void SetCustomKey(string key, double value)
    {
        _instance.SetCustomKey(key, value);
    }

    public void SetCustomKey(string key, string value)
    {
        _instance.SetCustomKey(key, value);
    }

    public void SetCustomKeys(IDictionary<string, object> customKeysAndValues)
    {
        var customKeysAndValuesBuilder = new CustomKeysAndValues.Builder();
        foreach(var (key, value) in customKeysAndValues) {
            switch(value) {
                case bool boolValue:
                    customKeysAndValuesBuilder.PutBoolean(key, boolValue);
                    break;
                case int intValue:
                    customKeysAndValuesBuilder.PutInt(key, intValue);
                    break;
                case long longValue:
                    customKeysAndValuesBuilder.PutLong(key, longValue);
                    break;
                case float floatValue:
                    customKeysAndValuesBuilder.PutFloat(key, floatValue);
                    break;
                case double doubleValue:
                    customKeysAndValuesBuilder.PutDouble(key, doubleValue);
                    break;
                case string stringValue:
                    customKeysAndValuesBuilder.PutString(key, stringValue);
                    break;
                default:
                    throw new ArgumentException("Unexpected customKey value type");
            }
        }
        _instance.SetCustomKeys(customKeysAndValuesBuilder.Build());
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
        _instance.RecordException(CrashlyticsException.Create(exception));
    }

    public bool DidCrashOnPreviousExecution()
    {
        return _instance.DidCrashOnPreviousExecution();
    }

    public async Task<bool> CheckForUnsentReportsAsync()
    {
        var result = await _instance.CheckForUnsentReports().AsAsync<Java.Lang.Boolean>();
        return (bool) result;
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