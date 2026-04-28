#if ANDROID
using AndroidRuntime = Android.Runtime;
#endif

namespace Plugin.Firebase.IntegrationTests;

internal static class IntegrationTestConfiguration
{
    public static bool IsFeatureEnabled(string environmentVariableName, string androidSystemPropertyName)
    {
        return string.Equals(
            GetConfigurationValue(environmentVariableName, androidSystemPropertyName),
            "1",
            StringComparison.Ordinal);
    }

    public static string GetEmulatorHost(string environmentVariableName, string androidSystemPropertyName)
    {
        var host = GetConfigurationValue(environmentVariableName, androidSystemPropertyName);
        return string.IsNullOrWhiteSpace(host)
            ? OperatingSystem.IsAndroid() ? "10.0.2.2" : "localhost"
            : host;
    }

    public static int GetEmulatorPort(
        string environmentVariableName,
        string androidSystemPropertyName,
        int defaultPort)
    {
        var portValue = GetConfigurationValue(environmentVariableName, androidSystemPropertyName);
        if(string.IsNullOrWhiteSpace(portValue)) {
            return defaultPort;
        }

        if(!int.TryParse(portValue, out var port)) {
            throw new InvalidOperationException(
                $"{environmentVariableName} must be an integer, but was '{portValue}'.");
        }

        return port;
    }

    private static string GetConfigurationValue(string environmentVariableName, string androidSystemPropertyName)
    {
        var environmentVariableValue = Environment.GetEnvironmentVariable(environmentVariableName);
        if(!string.IsNullOrWhiteSpace(environmentVariableValue)) {
            return environmentVariableValue;
        }

#if ANDROID
        return GetAndroidSystemProperty(androidSystemPropertyName);
#else
        return null;
#endif
    }

#if ANDROID
    private static string GetAndroidSystemProperty(string propertyName)
    {
        IntPtr? propertyValuePointer = null;

        try {
            var systemPropertiesClass = AndroidRuntime.JNIEnv.FindClass("android/os/SystemProperties");
            var getMethodId = AndroidRuntime.JNIEnv.GetStaticMethodID(
                systemPropertiesClass,
                "get",
                "(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;");

            using var propertyNameValue = new Java.Lang.String(propertyName);
            using var defaultValue = new Java.Lang.String(string.Empty);
            propertyValuePointer = AndroidRuntime.JNIEnv.CallStaticObjectMethod(
                systemPropertiesClass,
                getMethodId,
                new AndroidRuntime.JValue(propertyNameValue),
                new AndroidRuntime.JValue(defaultValue));

            return AndroidRuntime.JNIEnv.GetString(
                propertyValuePointer.Value,
                AndroidRuntime.JniHandleOwnership.DoNotTransfer);
        } catch {
            return null;
        } finally {
            if(propertyValuePointer.HasValue && propertyValuePointer.Value != IntPtr.Zero) {
                AndroidRuntime.JNIEnv.DeleteLocalRef(propertyValuePointer.Value);
            }
        }
    }
#endif
}
